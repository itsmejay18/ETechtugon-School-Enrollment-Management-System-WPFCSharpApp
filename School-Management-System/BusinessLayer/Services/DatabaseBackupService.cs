using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.DataLayer.Logging;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed partial class DatabaseBackupService
    {
        private const string BackupTypeFull = "Full";
        private const string BackupTypeIncremental = "Incremental";
        private const string BackupTypeDifferential = "Differential";
        private const string BackupFileSearchPattern = "*.smsbak";
        private const int DbCommandTimeoutSeconds = 300;

        private readonly DatabaseHelper _db;
        private readonly ActivityLogService _activityLogService;

        public DatabaseBackupService(DatabaseHelper db, ActivityLogService activityLogService = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _activityLogService = activityLogService;
        }

        public string GetDefaultBackupDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SchoolManagementSystem",
                "Backups");
        }

        public DatabaseBackupResult CreateBackup(string requestedType, string outputDirectory, int? userId, string username)
        {
            return CreateBackup(requestedType, outputDirectory, userId, username, CancellationToken.None);
        }

        public DatabaseBackupResult CreateBackup(string requestedType, string outputDirectory, int? userId, string username, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedRequestedType = NormalizeBackupType(requestedType);
            var effectiveType = normalizedRequestedType;
            var notes = new List<string>();

            try
            {
                var targetDirectory = NormalizeOutputDirectory(outputDirectory);
                Directory.CreateDirectory(targetDirectory);

                var currentSnapshot = CaptureCurrentSnapshot(cancellationToken);
                var latestSnapshot = LoadSnapshotState(GetLatestSnapshotPath());
                var lastFullSnapshot = LoadSnapshotState(GetLastFullSnapshotPath());

                DatabaseBackupSnapshotState baselineSnapshot = null;
                if (string.Equals(normalizedRequestedType, BackupTypeIncremental, StringComparison.OrdinalIgnoreCase))
                {
                    baselineSnapshot = latestSnapshot;
                    if (baselineSnapshot == null)
                    {
                        effectiveType = BackupTypeFull;
                        notes.Add("Incremental baseline not found. Created Full backup instead.");
                    }
                }
                else if (string.Equals(normalizedRequestedType, BackupTypeDifferential, StringComparison.OrdinalIgnoreCase))
                {
                    baselineSnapshot = lastFullSnapshot;
                    if (baselineSnapshot == null)
                    {
                        effectiveType = BackupTypeFull;
                        notes.Add("Differential baseline not found. Created Full backup instead.");
                    }
                }

                var backupId = Guid.NewGuid().ToString("N");
                var parentBackupId = string.Equals(effectiveType, BackupTypeFull, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : (baselineSnapshot == null ? null : baselineSnapshot.SourceBackupId);
                var rootFullBackupId = string.Equals(effectiveType, BackupTypeFull, StringComparison.OrdinalIgnoreCase)
                    ? backupId
                    : ResolveRootFullBackupId(baselineSnapshot);

                var package = BuildPackage(
                    currentSnapshot,
                    baselineSnapshot,
                    backupId,
                    normalizedRequestedType,
                    effectiveType,
                    parentBackupId,
                    rootFullBackupId,
                    userId,
                    username,
                    notes,
                    cancellationToken);

                var fileName = BuildBackupFileName(package.Metadata.CreatedAtUtc, effectiveType, backupId);
                var outputFilePath = Path.Combine(targetDirectory, fileName);
                SaveBackupPackage(outputFilePath, package);

                SaveSnapshotState(
                    new DatabaseBackupSnapshotState
                    {
                        SourceBackupId = backupId,
                        RootFullBackupId = rootFullBackupId,
                        CapturedAtUtc = DateTime.UtcNow,
                        Tables = CloneTables(currentSnapshot.Tables)
                    },
                    GetLatestSnapshotPath());

                if (string.Equals(effectiveType, BackupTypeFull, StringComparison.OrdinalIgnoreCase))
                {
                    SaveSnapshotState(
                        new DatabaseBackupSnapshotState
                        {
                            SourceBackupId = backupId,
                            RootFullBackupId = backupId,
                            CapturedAtUtc = DateTime.UtcNow,
                            Tables = CloneTables(currentSnapshot.Tables)
                        },
                        GetLastFullSnapshotPath());
                }

                var rowCount = CountRowsForResult(package);
                var result = new DatabaseBackupResult
                {
                    RequestedType = normalizedRequestedType,
                    EffectiveType = effectiveType,
                    BackupId = backupId,
                    OutputFilePath = outputFilePath,
                    TableCount = package.Tables == null ? 0 : package.Tables.Count,
                    RowCount = rowCount,
                    Notes = string.Join(" ", notes)
                };

                _activityLogService?.Log(
                    userId,
                    ResolveBackupAction(effectiveType),
                    AppConstants.Entities.Database,
                    null,
                    BuildBackupLogDetails(result));

                return result;
            }
            catch (Exception ex)
            {
                _activityLogService?.Log(
                    userId,
                    AppConstants.ActivityActions.BackupError,
                    AppConstants.Entities.Database,
                    null,
                    "Backup failed. Type=" + normalizedRequestedType + ". Error=" + SafeMessage(ex));

                FileLogger.LogError("DatabaseBackupService.CreateBackup", ex);
                throw;
            }
        }

        public DatabaseRestoreResult RestoreBackup(string backupFilePath, int? userId, string username)
        {
            return RestoreBackup(backupFilePath, userId, username, CancellationToken.None);
        }

        public DatabaseRestoreResult RestoreBackup(string backupFilePath, int? userId, string username, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                throw new ArgumentException("Backup file path is required.", nameof(backupFilePath));
            }

            var filePath = backupFilePath.Trim();
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Backup file does not exist: " + filePath);
            }

            try
            {
                var selectedPackage = LoadBackupPackage(filePath);
                var chain = ResolveRestoreChain(selectedPackage, filePath, cancellationToken);
                var snapshot = BuildSnapshotFromRestoreChain(chain, cancellationToken);
                ApplySnapshotToDatabase(snapshot, cancellationToken);

                var latestPackage = chain[chain.Count - 1];
                SaveSnapshotState(
                    new DatabaseBackupSnapshotState
                    {
                        SourceBackupId = latestPackage.Metadata.BackupId,
                        RootFullBackupId = latestPackage.Metadata.RootFullBackupId,
                        CapturedAtUtc = DateTime.UtcNow,
                        Tables = CloneTables(snapshot.Tables)
                    },
                    GetLatestSnapshotPath());

                if (string.Equals(NormalizeBackupType(latestPackage.Metadata.BackupType), BackupTypeFull, StringComparison.OrdinalIgnoreCase))
                {
                    SaveSnapshotState(
                        new DatabaseBackupSnapshotState
                        {
                            SourceBackupId = latestPackage.Metadata.BackupId,
                            RootFullBackupId = latestPackage.Metadata.BackupId,
                            CapturedAtUtc = DateTime.UtcNow,
                            Tables = CloneTables(snapshot.Tables)
                        },
                        GetLastFullSnapshotPath());
                }

                var result = new DatabaseRestoreResult
                {
                    SelectedBackupFilePath = filePath,
                    SelectedBackupType = NormalizeBackupType(selectedPackage.Metadata.BackupType),
                    AppliedBackupPackages = chain.Count,
                    RestoredTables = snapshot.Tables.Count,
                    RestoredRows = CountRows(snapshot.Tables)
                };

                _activityLogService?.Log(
                    userId,
                    AppConstants.ActivityActions.RestoreDatabase,
                    AppConstants.Entities.Database,
                    null,
                    "Restored from '" + filePath + "'. PackagesApplied=" + result.AppliedBackupPackages + ", Tables=" + result.RestoredTables + ", Rows=" + result.RestoredRows + ".");

                return result;
            }
            catch (Exception ex)
            {
                _activityLogService?.Log(
                    userId,
                    AppConstants.ActivityActions.RestoreError,
                    AppConstants.Entities.Database,
                    null,
                    "Restore failed for '" + filePath + "'. Error=" + SafeMessage(ex));

                FileLogger.LogError("DatabaseBackupService.RestoreBackup", ex);
                throw;
            }
        }

        private DatabaseBackupPackage BuildPackage(
            DatabaseBackupSnapshotState currentSnapshot,
            DatabaseBackupSnapshotState baselineSnapshot,
            string backupId,
            string requestedType,
            string effectiveType,
            string parentBackupId,
            string rootFullBackupId,
            int? userId,
            string username,
            IList<string> notes,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var builder = new MySqlConnectionStringBuilder(ConnectionStringProvider.GetDefault());
            var package = new DatabaseBackupPackage
            {
                Metadata = new DatabaseBackupMetadata
                {
                    BackupId = backupId,
                    BackupType = effectiveType,
                    ParentBackupId = parentBackupId,
                    RootFullBackupId = rootFullBackupId,
                    CreatedAtUtc = DateTime.UtcNow,
                    DatabaseName = builder.Database,
                    ServerName = builder.Server,
                    MachineName = Environment.MachineName,
                    CreatedByUserId = userId,
                    CreatedByUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim(),
                    Notes = BuildPackageNotes(requestedType, effectiveType, notes)
                }
            };

            if (string.Equals(effectiveType, BackupTypeFull, StringComparison.OrdinalIgnoreCase))
            {
                package.Tables = CloneTables(currentSnapshot.Tables);
                return package;
            }

            package.Tables = BuildDeltaTables(
                currentSnapshot.Tables,
                baselineSnapshot == null ? null : baselineSnapshot.Tables,
                cancellationToken);
            return package;
        }

        private DatabaseBackupSnapshotState CaptureCurrentSnapshot(CancellationToken cancellationToken)
        {
            var snapshot = new DatabaseBackupSnapshotState
            {
                CapturedAtUtc = DateTime.UtcNow,
                Tables = new List<DatabaseBackupTableData>()
            };

            foreach (var tableName in GetDatabaseTableNames())
            {
                cancellationToken.ThrowIfCancellationRequested();
                snapshot.Tables.Add(CaptureTableSnapshot(tableName, cancellationToken));
            }

            return snapshot;
        }

        private IList<string> GetDatabaseTableNames()
        {
            const string sql = @"
SELECT table_name
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_type = 'BASE TABLE'
ORDER BY table_name;";

            var dt = _db.ExecuteDataTable(sql, CommandType.Text, null);
            var list = new List<string>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                var tableName = Convert.ToString(row["table_name"]);
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                list.Add(tableName.Trim());
            }

            return list;
        }

        private DatabaseBackupTableData CaptureTableSnapshot(string tableName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tableData = new DatabaseBackupTableData
            {
                TableName = tableName,
                PrimaryKeys = GetPrimaryKeys(tableName).ToList()
            };

            var sql = "SELECT * FROM " + QuoteIdentifier(tableName) + ";";
            var dt = _db.ExecuteDataTable(sql, CommandType.Text, null);

            foreach (DataColumn col in dt.Columns)
            {
                if (col == null || string.IsNullOrWhiteSpace(col.ColumnName))
                {
                    continue;
                }

                tableData.Columns.Add(col.ColumnName);
            }

            foreach (DataRow row in dt.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (row == null)
                {
                    continue;
                }

                tableData.Rows.Add(BuildRowData(row, dt.Columns));
            }

            return tableData;
        }

        private IList<string> GetPrimaryKeys(string tableName)
        {
            const string sql = @"
SELECT k.COLUMN_NAME
FROM information_schema.table_constraints tc
INNER JOIN information_schema.key_column_usage k
    ON tc.constraint_name = k.constraint_name
   AND tc.table_schema = k.table_schema
   AND tc.table_name = k.table_name
WHERE tc.table_schema = DATABASE()
  AND tc.table_name = @TableName
  AND tc.constraint_type = 'PRIMARY KEY'
ORDER BY k.ORDINAL_POSITION;";

            var dt = _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@TableName", tableName)
                });

            var keys = new List<string>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                var key = Convert.ToString(row["COLUMN_NAME"]);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                keys.Add(key.Trim());
            }

            return keys;
        }

        private static DatabaseBackupRowData BuildRowData(DataRow row, DataColumnCollection columns)
        {
            var rowData = new DatabaseBackupRowData();
            foreach (DataColumn column in columns)
            {
                if (column == null || string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    continue;
                }

                var value = row[column];
                var isNull = value == null || value == DBNull.Value;
                rowData.Fields.Add(
                    new DatabaseBackupFieldData
                    {
                        Name = column.ColumnName,
                        TypeName = column.DataType == null ? typeof(string).FullName : column.DataType.FullName,
                        IsNull = isNull,
                        Value = isNull ? null : SerializeFieldValue(value)
                    });
            }

            return rowData;
        }

        private static string SerializeFieldValue(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            var bytes = value as byte[];
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }

            if (value is DateTime)
            {
                return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is TimeSpan)
            {
                return ((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture);
            }

            if (value is bool)
            {
                return ((bool)value) ? "1" : "0";
            }

            var formattable = value as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static List<DatabaseBackupTableData> BuildDeltaTables(
            IList<DatabaseBackupTableData> currentTables,
            IList<DatabaseBackupTableData> baselineTables,
            CancellationToken cancellationToken)
        {
            var baselineByName = new Dictionary<string, DatabaseBackupTableData>(StringComparer.OrdinalIgnoreCase);
            if (baselineTables != null)
            {
                foreach (var baselineTable in baselineTables)
                {
                    if (baselineTable == null || string.IsNullOrWhiteSpace(baselineTable.TableName))
                    {
                        continue;
                    }

                    baselineByName[baselineTable.TableName] = baselineTable;
                }
            }

            var deltaTables = new List<DatabaseBackupTableData>();
            foreach (var currentTable in currentTables ?? new List<DatabaseBackupTableData>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (currentTable == null || string.IsNullOrWhiteSpace(currentTable.TableName))
                {
                    continue;
                }

                DatabaseBackupTableData baselineTable;
                baselineByName.TryGetValue(currentTable.TableName, out baselineTable);

                var delta = BuildTableDelta(currentTable, baselineTable);
                if (delta != null)
                {
                    deltaTables.Add(delta);
                }
            }

            return deltaTables;
        }

        private static DatabaseBackupTableData BuildTableDelta(DatabaseBackupTableData currentTable, DatabaseBackupTableData baselineTable)
        {
            var keyColumns = ResolveKeyColumns(currentTable, baselineTable);
            var currentMap = BuildRowMap(currentTable == null ? null : currentTable.Rows, keyColumns);
            var baselineMap = BuildRowMap(baselineTable == null ? null : baselineTable.Rows, keyColumns);

            var upserts = new List<DatabaseBackupRowData>();
            foreach (var pair in currentMap)
            {
                DatabaseBackupRowData oldRow;
                if (!baselineMap.TryGetValue(pair.Key, out oldRow) || !RowsEquivalent(pair.Value, oldRow))
                {
                    upserts.Add(CloneRow(pair.Value));
                }
            }

            var deletes = new List<DatabaseBackupRowData>();
            foreach (var pair in baselineMap)
            {
                if (!currentMap.ContainsKey(pair.Key))
                {
                    deletes.Add(BuildKeyRow(pair.Value, keyColumns));
                }
            }

            if (upserts.Count == 0 && deletes.Count == 0)
            {
                return null;
            }

            return new DatabaseBackupTableData
            {
                TableName = currentTable.TableName,
                Columns = CloneList(currentTable.Columns),
                PrimaryKeys = CloneList(keyColumns),
                Rows = upserts,
                DeletedKeys = deletes
            };
        }

        private static IList<string> ResolveKeyColumns(DatabaseBackupTableData currentTable, DatabaseBackupTableData baselineTable)
        {
            var keys = new List<string>();

            if (currentTable != null && currentTable.PrimaryKeys != null && currentTable.PrimaryKeys.Count > 0)
            {
                keys.AddRange(currentTable.PrimaryKeys.Where(k => !string.IsNullOrWhiteSpace(k)));
            }
            else if (baselineTable != null && baselineTable.PrimaryKeys != null && baselineTable.PrimaryKeys.Count > 0)
            {
                keys.AddRange(baselineTable.PrimaryKeys.Where(k => !string.IsNullOrWhiteSpace(k)));
            }
            else if (currentTable != null && currentTable.Columns != null && currentTable.Columns.Count > 0)
            {
                keys.AddRange(currentTable.Columns.Where(c => !string.IsNullOrWhiteSpace(c)));
            }
            else if (baselineTable != null && baselineTable.Columns != null && baselineTable.Columns.Count > 0)
            {
                keys.AddRange(baselineTable.Columns.Where(c => !string.IsNullOrWhiteSpace(c)));
            }

            var distinct = new List<string>();
            foreach (var key in keys)
            {
                if (distinct.Any(existing => string.Equals(existing, key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                distinct.Add(key);
            }

            return distinct;
        }

        private static Dictionary<string, DatabaseBackupRowData> BuildRowMap(IList<DatabaseBackupRowData> rows, IList<string> keyColumns)
        {
            var map = new Dictionary<string, DatabaseBackupRowData>(StringComparer.Ordinal);
            foreach (var row in rows ?? new List<DatabaseBackupRowData>())
            {
                if (row == null)
                {
                    continue;
                }

                var key = BuildRowIdentity(row, keyColumns);
                map[key] = row;
            }

            return map;
        }

        private static string BuildRowIdentity(DatabaseBackupRowData row, IList<string> keyColumns)
        {
            var columns = keyColumns != null && keyColumns.Count > 0
                ? keyColumns
                : (row == null || row.Fields == null
                    ? new List<string>()
                    : row.Fields.Select(f => f.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList());

            var sb = new StringBuilder();
            foreach (var column in columns)
            {
                var field = FindField(row, column);
                sb.Append(column == null ? string.Empty : column.Trim().ToLowerInvariant());
                sb.Append('=');
                sb.Append(SerializeFieldIdentity(field));
                sb.Append('|');
            }

            return sb.ToString();
        }

        private static string SerializeFieldIdentity(DatabaseBackupFieldData field)
        {
            if (field == null)
            {
                return "<missing>";
            }

            if (field.IsNull)
            {
                return "<null>";
            }

            return (field.TypeName ?? string.Empty) + ":" + (field.Value ?? string.Empty);
        }

        private static DatabaseBackupFieldData FindField(DatabaseBackupRowData row, string columnName)
        {
            if (row == null || row.Fields == null || string.IsNullOrWhiteSpace(columnName))
            {
                return null;
            }

            foreach (var field in row.Fields)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.Name))
                {
                    continue;
                }

                if (string.Equals(field.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return field;
                }
            }

            return null;
        }

        private static bool RowsEquivalent(DatabaseBackupRowData left, DatabaseBackupRowData right)
        {
            var leftMap = ToFieldMap(left);
            var rightMap = ToFieldMap(right);

            if (leftMap.Count != rightMap.Count)
            {
                return false;
            }

            foreach (var leftPair in leftMap)
            {
                DatabaseBackupFieldData rightField;
                if (!rightMap.TryGetValue(leftPair.Key, out rightField))
                {
                    return false;
                }

                if (!FieldsEquivalent(leftPair.Value, rightField))
                {
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, DatabaseBackupFieldData> ToFieldMap(DatabaseBackupRowData row)
        {
            var map = new Dictionary<string, DatabaseBackupFieldData>(StringComparer.OrdinalIgnoreCase);
            if (row == null || row.Fields == null)
            {
                return map;
            }

            foreach (var field in row.Fields)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.Name))
                {
                    continue;
                }

                map[field.Name] = field;
            }

            return map;
        }

        private static bool FieldsEquivalent(DatabaseBackupFieldData left, DatabaseBackupFieldData right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.IsNull != right.IsNull)
            {
                return false;
            }

            if (!string.Equals(left.TypeName ?? string.Empty, right.TypeName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (left.IsNull)
            {
                return true;
            }

            return string.Equals(left.Value ?? string.Empty, right.Value ?? string.Empty, StringComparison.Ordinal);
        }

        private static DatabaseBackupRowData BuildKeyRow(DatabaseBackupRowData sourceRow, IList<string> keyColumns)
        {
            var keyRow = new DatabaseBackupRowData();
            if (sourceRow == null)
            {
                return keyRow;
            }

            if (keyColumns == null || keyColumns.Count == 0)
            {
                keyRow.Fields = CloneFields(sourceRow.Fields);
                return keyRow;
            }

            foreach (var keyColumn in keyColumns)
            {
                var field = FindField(sourceRow, keyColumn);
                if (field != null)
                {
                    keyRow.Fields.Add(CloneField(field));
                }
            }

            return keyRow;
        }

        private static string BuildBackupFileName(DateTime timestampUtc, string backupType, string backupId)
        {
            var safeType = string.IsNullOrWhiteSpace(backupType) ? BackupTypeFull : backupType.Trim();
            var idPart = string.IsNullOrWhiteSpace(backupId) ? Guid.NewGuid().ToString("N").Substring(0, 8) : backupId.Substring(0, Math.Min(8, backupId.Length));
            return timestampUtc.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)
                + "_"
                + safeType
                + "_"
                + idPart
                + ".smsbak";
        }

        private static void SaveXml<T>(string path, T payload)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(fs, payload);
            }
        }

        private static T LoadXml<T>(string path) where T : class
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(T));
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return serializer.Deserialize(fs) as T;
            }
        }

        private static string GetBackupStateDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SchoolManagementSystem",
                "backup-state");
        }

        private static string GetLatestSnapshotPath()
        {
            return Path.Combine(GetBackupStateDirectory(), "latest_snapshot.xml");
        }

        private static string GetLastFullSnapshotPath()
        {
            return Path.Combine(GetBackupStateDirectory(), "last_full_snapshot.xml");
        }

        private static void SaveSnapshotState(DatabaseBackupSnapshotState state, string path)
        {
            if (state == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            SaveXml(path, state);
        }

        private static DatabaseBackupSnapshotState LoadSnapshotState(string path)
        {
            try
            {
                return LoadXml<DatabaseBackupSnapshotState>(path);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseBackupService.LoadSnapshotState", ex);
                return null;
            }
        }

        private static string NormalizeBackupType(string backupType)
        {
            if (string.IsNullOrWhiteSpace(backupType))
            {
                return BackupTypeFull;
            }

            var normalized = backupType.Trim();
            if (string.Equals(normalized, "incremental", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "inc", StringComparison.OrdinalIgnoreCase))
            {
                return BackupTypeIncremental;
            }

            if (string.Equals(normalized, "differential", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "diff", StringComparison.OrdinalIgnoreCase))
            {
                return BackupTypeDifferential;
            }

            return BackupTypeFull;
        }

        private static string NormalizeOutputDirectory(string outputDirectory)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "SchoolManagementSystem",
                    "Backups");
            }

            return outputDirectory.Trim();
        }

        private static string ResolveRootFullBackupId(DatabaseBackupSnapshotState baselineSnapshot)
        {
            if (baselineSnapshot == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(baselineSnapshot.RootFullBackupId))
            {
                return baselineSnapshot.RootFullBackupId;
            }

            return baselineSnapshot.SourceBackupId;
        }

        private static string BuildPackageNotes(string requestedType, string effectiveType, IEnumerable<string> notes)
        {
            var noteList = new List<string>();
            if (!string.Equals(requestedType, effectiveType, StringComparison.OrdinalIgnoreCase))
            {
                noteList.Add("RequestedType=" + requestedType + ", EffectiveType=" + effectiveType + ".");
            }

            foreach (var note in notes ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(note))
                {
                    continue;
                }

                noteList.Add(note.Trim());
            }

            return string.Join(" ", noteList);
        }

        private static string ResolveBackupAction(string backupType)
        {
            var normalized = NormalizeBackupType(backupType);
            if (string.Equals(normalized, BackupTypeIncremental, StringComparison.OrdinalIgnoreCase))
            {
                return AppConstants.ActivityActions.BackupIncremental;
            }

            if (string.Equals(normalized, BackupTypeDifferential, StringComparison.OrdinalIgnoreCase))
            {
                return AppConstants.ActivityActions.BackupDifferential;
            }

            return AppConstants.ActivityActions.BackupFull;
        }

        private static string BuildBackupLogDetails(DatabaseBackupResult result)
        {
            if (result == null)
            {
                return "Database backup completed.";
            }

            var details = "Backup saved to '" + (result.OutputFilePath ?? string.Empty) + "'. "
                + "RequestedType=" + (result.RequestedType ?? string.Empty) + ", "
                + "EffectiveType=" + (result.EffectiveType ?? string.Empty) + ", "
                + "Tables=" + result.TableCount + ", "
                + "Rows=" + result.RowCount + ".";

            if (!string.IsNullOrWhiteSpace(result.Notes))
            {
                details += " " + result.Notes.Trim();
            }

            return details;
        }

        private static string SafeMessage(Exception ex)
        {
            if (ex == null || string.IsNullOrWhiteSpace(ex.Message))
            {
                return "Unknown error.";
            }

            var message = ex.Message.Trim();
            return message.Length > 240 ? message.Substring(0, 240) + "..." : message;
        }

        private static int CountRowsForResult(DatabaseBackupPackage package)
        {
            if (package == null || package.Tables == null)
            {
                return 0;
            }

            var normalizedType = NormalizeBackupType(package.Metadata == null ? null : package.Metadata.BackupType);
            if (string.Equals(normalizedType, BackupTypeFull, StringComparison.OrdinalIgnoreCase))
            {
                return CountRows(package.Tables);
            }

            var count = 0;
            foreach (var table in package.Tables)
            {
                if (table == null)
                {
                    continue;
                }

                count += table.Rows == null ? 0 : table.Rows.Count;
                count += table.DeletedKeys == null ? 0 : table.DeletedKeys.Count;
            }

            return count;
        }

        private static int CountRows(IEnumerable<DatabaseBackupTableData> tables)
        {
            var count = 0;
            foreach (var table in tables ?? Enumerable.Empty<DatabaseBackupTableData>())
            {
                if (table == null || table.Rows == null)
                {
                    continue;
                }

                count += table.Rows.Count;
            }

            return count;
        }

        private static DatabaseBackupPackage LoadBackupPackage(string path)
        {
            var package = ReadBackupPackage(path);
            if (package == null || package.Metadata == null || string.IsNullOrWhiteSpace(package.Metadata.BackupId))
            {
                throw new InvalidOperationException("Invalid backup file: " + path);
            }

            package.Metadata.BackupType = NormalizeBackupType(package.Metadata.BackupType);
            if (package.Tables == null)
            {
                package.Tables = new List<DatabaseBackupTableData>();
            }

            return package;
        }

        private static List<DatabaseBackupPackage> ResolveRestoreChain(DatabaseBackupPackage selectedPackage, string selectedFilePath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var selectedType = NormalizeBackupType(selectedPackage.Metadata.BackupType);
            if (string.Equals(selectedType, BackupTypeFull, StringComparison.OrdinalIgnoreCase))
            {
                return new List<DatabaseBackupPackage> { selectedPackage };
            }

            var directory = Path.GetDirectoryName(selectedFilePath);
            var packageById = LoadBackupPackagesInDirectory(directory, cancellationToken);
            packageById[selectedPackage.Metadata.BackupId] = selectedPackage;

            if (string.Equals(selectedType, BackupTypeDifferential, StringComparison.OrdinalIgnoreCase))
            {
                var parent = ResolveParentPackage(selectedPackage, packageById);
                if (!string.Equals(NormalizeBackupType(parent.Metadata.BackupType), BackupTypeFull, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Differential backup parent must be a Full backup.");
                }

                return new List<DatabaseBackupPackage> { parent, selectedPackage };
            }

            if (!string.Equals(selectedType, BackupTypeIncremental, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Unsupported backup type: " + selectedType);
            }

            var chainReversed = new List<DatabaseBackupPackage> { selectedPackage };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                selectedPackage.Metadata.BackupId
            };

            var current = selectedPackage;
            while (!string.Equals(NormalizeBackupType(current.Metadata.BackupType), BackupTypeFull, StringComparison.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var parent = ResolveParentPackage(current, packageById);
                if (parent == null || parent.Metadata == null || string.IsNullOrWhiteSpace(parent.Metadata.BackupId))
                {
                    throw new InvalidOperationException("Missing parent backup required for incremental restore.");
                }

                if (!visited.Add(parent.Metadata.BackupId))
                {
                    throw new InvalidOperationException("Backup dependency loop detected.");
                }

                chainReversed.Add(parent);
                current = parent;
            }

            chainReversed.Reverse();
            return chainReversed;
        }

        private static Dictionary<string, DatabaseBackupPackage> LoadBackupPackagesInDirectory(string directory, CancellationToken cancellationToken)
        {
            var map = new Dictionary<string, DatabaseBackupPackage>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return map;
            }

            foreach (var path in Directory.GetFiles(directory, BackupFileSearchPattern, SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var package = LoadBackupPackage(path);
                    map[package.Metadata.BackupId] = package;
                }
                catch
                {
                    // Ignore unreadable backup files when resolving chain.
                }
            }

            return map;
        }

        private static DatabaseBackupPackage ResolveParentPackage(DatabaseBackupPackage package, IDictionary<string, DatabaseBackupPackage> packageById)
        {
            if (package == null || package.Metadata == null)
            {
                throw new InvalidOperationException("Backup metadata is missing.");
            }

            var parentBackupId = package.Metadata.ParentBackupId;
            if (string.IsNullOrWhiteSpace(parentBackupId))
            {
                throw new InvalidOperationException("Backup '" + package.Metadata.BackupId + "' has no parent information.");
            }

            DatabaseBackupPackage parent;
            if (!packageById.TryGetValue(parentBackupId, out parent) || parent == null)
            {
                throw new InvalidOperationException("Required parent backup '" + parentBackupId + "' was not found in the selected backup folder.");
            }

            return parent;
        }

        private static DatabaseBackupSnapshotState BuildSnapshotFromRestoreChain(IList<DatabaseBackupPackage> chain, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (chain == null || chain.Count == 0)
            {
                throw new InvalidOperationException("No backup chain available for restore.");
            }

            var basePackage = chain[0];
            if (!string.Equals(NormalizeBackupType(basePackage.Metadata.BackupType), BackupTypeFull, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Restore chain must start with a Full backup.");
            }

            var snapshot = new DatabaseBackupSnapshotState
            {
                SourceBackupId = chain[chain.Count - 1].Metadata.BackupId,
                RootFullBackupId = chain[chain.Count - 1].Metadata.RootFullBackupId,
                CapturedAtUtc = DateTime.UtcNow,
                Tables = CloneTables(basePackage.Tables)
            };

            var tableByName = new Dictionary<string, DatabaseBackupTableData>(StringComparer.OrdinalIgnoreCase);
            foreach (var table in snapshot.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                {
                    continue;
                }

                tableByName[table.TableName] = table;
            }

            for (var i = 1; i < chain.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var package = chain[i];
                foreach (var deltaTable in package.Tables ?? new List<DatabaseBackupTableData>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (deltaTable == null || string.IsNullOrWhiteSpace(deltaTable.TableName))
                    {
                        continue;
                    }

                    DatabaseBackupTableData existingTable;
                    if (!tableByName.TryGetValue(deltaTable.TableName, out existingTable))
                    {
                        existingTable = new DatabaseBackupTableData
                        {
                            TableName = deltaTable.TableName,
                            Columns = CloneList(deltaTable.Columns),
                            PrimaryKeys = CloneList(deltaTable.PrimaryKeys),
                            Rows = new List<DatabaseBackupRowData>()
                        };

                        tableByName[deltaTable.TableName] = existingTable;
                    }

                    if ((existingTable.Columns == null || existingTable.Columns.Count == 0) && deltaTable.Columns != null && deltaTable.Columns.Count > 0)
                    {
                        existingTable.Columns = CloneList(deltaTable.Columns);
                    }

                    if ((existingTable.PrimaryKeys == null || existingTable.PrimaryKeys.Count == 0) && deltaTable.PrimaryKeys != null && deltaTable.PrimaryKeys.Count > 0)
                    {
                        existingTable.PrimaryKeys = CloneList(deltaTable.PrimaryKeys);
                    }

                    var keyColumns = ResolveKeyColumns(existingTable, deltaTable);
                    var rowMap = BuildRowMap(existingTable.Rows, keyColumns);

                    foreach (var deleteRow in deltaTable.DeletedKeys ?? new List<DatabaseBackupRowData>())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var deleteKey = BuildRowIdentity(deleteRow, keyColumns);
                        rowMap.Remove(deleteKey);
                    }

                    foreach (var upsertRow in deltaTable.Rows ?? new List<DatabaseBackupRowData>())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var upsertKey = BuildRowIdentity(upsertRow, keyColumns);
                        rowMap[upsertKey] = CloneRow(upsertRow);
                    }

                    var orderedRows = rowMap
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair => pair.Value)
                        .ToList();

                    existingTable.Rows = orderedRows;
                    existingTable.DeletedKeys = new List<DatabaseBackupRowData>();
                }
            }

            snapshot.Tables = tableByName.Values.OrderBy(t => t.TableName, StringComparer.OrdinalIgnoreCase).ToList();
            return snapshot;
        }

        private static List<DatabaseBackupTableData> CloneTables(IEnumerable<DatabaseBackupTableData> tables)
        {
            var cloned = new List<DatabaseBackupTableData>();
            foreach (var table in tables ?? Enumerable.Empty<DatabaseBackupTableData>())
            {
                cloned.Add(CloneTable(table));
            }

            return cloned;
        }

        private static DatabaseBackupTableData CloneTable(DatabaseBackupTableData source)
        {
            if (source == null)
            {
                return new DatabaseBackupTableData();
            }

            var table = new DatabaseBackupTableData
            {
                TableName = source.TableName,
                Columns = CloneList(source.Columns),
                PrimaryKeys = CloneList(source.PrimaryKeys),
                Rows = new List<DatabaseBackupRowData>(),
                DeletedKeys = new List<DatabaseBackupRowData>()
            };

            foreach (var row in source.Rows ?? new List<DatabaseBackupRowData>())
            {
                table.Rows.Add(CloneRow(row));
            }

            foreach (var keyRow in source.DeletedKeys ?? new List<DatabaseBackupRowData>())
            {
                table.DeletedKeys.Add(CloneRow(keyRow));
            }

            return table;
        }

        private static DatabaseBackupRowData CloneRow(DatabaseBackupRowData source)
        {
            var row = new DatabaseBackupRowData
            {
                Fields = CloneFields(source == null ? null : source.Fields)
            };
            return row;
        }

        private static List<DatabaseBackupFieldData> CloneFields(IEnumerable<DatabaseBackupFieldData> source)
        {
            var fields = new List<DatabaseBackupFieldData>();
            foreach (var field in source ?? Enumerable.Empty<DatabaseBackupFieldData>())
            {
                fields.Add(CloneField(field));
            }

            return fields;
        }

        private static DatabaseBackupFieldData CloneField(DatabaseBackupFieldData source)
        {
            if (source == null)
            {
                return new DatabaseBackupFieldData();
            }

            return new DatabaseBackupFieldData
            {
                Name = source.Name,
                TypeName = source.TypeName,
                IsNull = source.IsNull,
                Value = source.Value
            };
        }

        private static List<string> CloneList(IEnumerable<string> values)
        {
            var list = new List<string>();
            foreach (var value in values ?? Enumerable.Empty<string>())
            {
                list.Add(value);
            }

            return list;
        }

        private void ApplySnapshotToDatabase(DatabaseBackupSnapshotState snapshot, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (snapshot == null || snapshot.Tables == null)
            {
                throw new InvalidOperationException("Backup snapshot is empty.");
            }

            using (var conn = new MySqlConnection(ConnectionStringProvider.GetDefault()))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var fkDisabled = false;
                    try
                    {
                        ExecuteCommand(conn, tx, "SET FOREIGN_KEY_CHECKS = 0;");
                        fkDisabled = true;

                        foreach (var table in snapshot.Tables)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                            {
                                continue;
                            }

                            ExecuteCommand(conn, tx, "DELETE FROM " + QuoteIdentifier(table.TableName) + ";");
                        }

                        foreach (var table in snapshot.Tables)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            InsertTableRows(conn, tx, table, cancellationToken);
                        }

                        ExecuteCommand(conn, tx, "SET FOREIGN_KEY_CHECKS = 1;");
                        fkDisabled = false;

                        tx.Commit();
                    }
                    catch
                    {
                        if (fkDisabled)
                        {
                            try
                            {
                                ExecuteCommand(conn, tx, "SET FOREIGN_KEY_CHECKS = 1;");
                            }
                            catch
                            {
                                // Ignore FK-reset failure during rollback.
                            }
                        }

                        try
                        {
                            tx.Rollback();
                        }
                        catch
                        {
                            // Ignore rollback failure.
                        }

                        throw;
                    }
                }
            }
        }

        private static void InsertTableRows(MySqlConnection conn, MySqlTransaction tx, DatabaseBackupTableData table, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (table == null || string.IsNullOrWhiteSpace(table.TableName) || table.Rows == null || table.Rows.Count == 0)
            {
                return;
            }

            var columns = ResolveInsertColumns(table);
            if (columns.Count == 0)
            {
                return;
            }

            var quotedColumns = columns.Select(QuoteIdentifier).ToList();
            var parameterNames = columns.Select((c, i) => "@p" + i.ToString(CultureInfo.InvariantCulture)).ToList();
            var sql = "INSERT INTO "
                + QuoteIdentifier(table.TableName)
                + " ("
                + string.Join(", ", quotedColumns)
                + ") VALUES ("
                + string.Join(", ", parameterNames)
                + ");";

            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = DbCommandTimeoutSeconds;

                foreach (var row in table.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    cmd.Parameters.Clear();
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var field = FindField(row, columns[i]);
                        var value = ConvertFieldToClrValue(field);
                        cmd.Parameters.AddWithValue(parameterNames[i], value);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static IList<string> ResolveInsertColumns(DatabaseBackupTableData table)
        {
            var columns = new List<string>();
            foreach (var column in table.Columns ?? new List<string>())
            {
                if (string.IsNullOrWhiteSpace(column))
                {
                    continue;
                }

                if (columns.Any(c => string.Equals(c, column, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                columns.Add(column.Trim());
            }

            if (columns.Count > 0)
            {
                return columns;
            }

            var firstRow = table.Rows == null || table.Rows.Count == 0 ? null : table.Rows[0];
            foreach (var field in firstRow == null ? new List<DatabaseBackupFieldData>() : firstRow.Fields)
            {
                if (field == null || string.IsNullOrWhiteSpace(field.Name))
                {
                    continue;
                }

                columns.Add(field.Name.Trim());
            }

            return columns;
        }

        private static object ConvertFieldToClrValue(DatabaseBackupFieldData field)
        {
            if (field == null || field.IsNull)
            {
                return DBNull.Value;
            }

            var value = field.Value ?? string.Empty;
            var typeName = string.IsNullOrWhiteSpace(field.TypeName) ? typeof(string).FullName : field.TypeName.Trim();

            try
            {
                switch (typeName)
                {
                    case "System.Boolean":
                        if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)) return true;
                        if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)) return false;
                        bool boolValue;
                        return bool.TryParse(value, out boolValue) ? boolValue : false;
                    case "System.SByte":
                        return sbyte.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Byte":
                        return byte.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Int16":
                        return short.Parse(value, CultureInfo.InvariantCulture);
                    case "System.UInt16":
                        return ushort.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Int32":
                        return int.Parse(value, CultureInfo.InvariantCulture);
                    case "System.UInt32":
                        return uint.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Int64":
                        return long.Parse(value, CultureInfo.InvariantCulture);
                    case "System.UInt64":
                        return ulong.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Single":
                        return float.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Double":
                        return double.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Decimal":
                        return decimal.Parse(value, CultureInfo.InvariantCulture);
                    case "System.DateTime":
                        DateTime dt;
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                        {
                            return dt;
                        }
                        return DateTime.Parse(value, CultureInfo.InvariantCulture);
                    case "System.TimeSpan":
                        TimeSpan ts;
                        if (TimeSpan.TryParseExact(value, "c", CultureInfo.InvariantCulture, out ts))
                        {
                            return ts;
                        }
                        return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                    case "System.Byte[]":
                        return string.IsNullOrWhiteSpace(value) ? new byte[0] : Convert.FromBase64String(value);
                    case "System.Guid":
                        return Guid.Parse(value);
                    default:
                        return value;
                }
            }
            catch
            {
                return value;
            }
        }

        private static string QuoteIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Identifier is required.", nameof(identifier));
            }

            return "`" + identifier.Trim().Replace("`", "``") + "`";
        }

        private static void ExecuteCommand(MySqlConnection conn, MySqlTransaction tx, string sql)
        {
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = DbCommandTimeoutSeconds;
                cmd.ExecuteNonQuery();
            }
        }
    }
}

