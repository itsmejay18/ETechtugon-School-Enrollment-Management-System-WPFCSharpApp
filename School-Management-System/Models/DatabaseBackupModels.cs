using System;
using System.Collections.Generic;

namespace School_Management_System.Models
{
    public sealed class DatabaseBackupEnvelope
    {
        public string FormatVersion { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CipherAlgorithm { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string KeyFingerprint { get; set; }
        public string InitializationVector { get; set; }
        public string CipherText { get; set; }
        public string Signature { get; set; }
    }

    public sealed class DatabaseBackupPackage
    {
        public DatabaseBackupPackage()
        {
            Metadata = new DatabaseBackupMetadata();
            Tables = new List<DatabaseBackupTableData>();
        }

        public DatabaseBackupMetadata Metadata { get; set; }
        public List<DatabaseBackupTableData> Tables { get; set; }
    }

    public sealed class DatabaseBackupMetadata
    {
        public string BackupId { get; set; }
        public string BackupType { get; set; }
        public string ParentBackupId { get; set; }
        public string RootFullBackupId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string MachineName { get; set; }
        public int? CreatedByUserId { get; set; }
        public string CreatedByUsername { get; set; }
        public string Notes { get; set; }
    }

    public sealed class DatabaseBackupTableData
    {
        public DatabaseBackupTableData()
        {
            Columns = new List<string>();
            PrimaryKeys = new List<string>();
            Rows = new List<DatabaseBackupRowData>();
            DeletedKeys = new List<DatabaseBackupRowData>();
        }

        public string TableName { get; set; }
        public List<string> Columns { get; set; }
        public List<string> PrimaryKeys { get; set; }
        public List<DatabaseBackupRowData> Rows { get; set; }
        public List<DatabaseBackupRowData> DeletedKeys { get; set; }
    }

    public sealed class DatabaseBackupRowData
    {
        public DatabaseBackupRowData()
        {
            Fields = new List<DatabaseBackupFieldData>();
        }

        public List<DatabaseBackupFieldData> Fields { get; set; }
    }

    public sealed class DatabaseBackupFieldData
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsNull { get; set; }
        public string Value { get; set; }
    }

    public sealed class DatabaseBackupSnapshotState
    {
        public DatabaseBackupSnapshotState()
        {
            Tables = new List<DatabaseBackupTableData>();
        }

        public string SourceBackupId { get; set; }
        public string RootFullBackupId { get; set; }
        public DateTime CapturedAtUtc { get; set; }
        public List<DatabaseBackupTableData> Tables { get; set; }
    }

    public sealed class DatabaseBackupResult
    {
        public string RequestedType { get; set; }
        public string EffectiveType { get; set; }
        public string BackupId { get; set; }
        public string OutputFilePath { get; set; }
        public int TableCount { get; set; }
        public int RowCount { get; set; }
        public string Notes { get; set; }
    }

    public sealed class DatabaseRestoreResult
    {
        public string SelectedBackupFilePath { get; set; }
        public string SelectedBackupType { get; set; }
        public int AppliedBackupPackages { get; set; }
        public int RestoredTables { get; set; }
        public int RestoredRows { get; set; }
    }
}
