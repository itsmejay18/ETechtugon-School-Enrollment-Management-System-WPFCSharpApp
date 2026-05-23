using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.DataLayer.Logging;

namespace School_Management_System.DataLayer
{
    public sealed class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            Guard.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            _connectionString = connectionString;
        }

        public static DatabaseHelper FromConfig()
        {
            var connectionString = GetPreferredConnectionString();
            return new DatabaseHelper(ResolveBestConnectionString(connectionString));
        }

        private static string GetPreferredConnectionString()
        {
            if (IsOnlineMode())
            {
                var directOnline = BuildDirectOnlineConnectionString();
                if (!string.IsNullOrWhiteSpace(directOnline))
                {
                    return directOnline;
                }
            }

            return ConnectionStringProvider.GetDefault();
        }

        public bool TestConnection(out string errorMessage)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    errorMessage = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                FileLogger.LogError("DatabaseHelper.TestConnection", ex);
                return false;
            }
        }

        private static string ResolveBestConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            if (!IsOnlineMode())
            {
                return connectionString;
            }

            FileLogger.LogInfo("DatabaseHelper.ResolveBestConnectionString: Online mode detected. Probing connection candidates.");
            foreach (var candidate in BuildOnlineCandidates(connectionString))
            {
                FileLogger.LogInfo("DatabaseHelper.ResolveBestConnectionString: Trying " + DescribeConnection(candidate));
                if (CanOpen(candidate))
                {
                    FileLogger.LogInfo("DatabaseHelper.ResolveBestConnectionString: Connected using " + DescribeConnection(candidate));
                    return candidate;
                }
            }

            FileLogger.LogInfo("DatabaseHelper.ResolveBestConnectionString: All online candidates failed. Falling back to original connection string.");
            return connectionString;
        }

        private static IEnumerable<string> BuildOnlineCandidates(string connectionString)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in BuildOnlineCandidatesInternal(connectionString))
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (seen.Add(candidate))
                {
                    yield return candidate;
                }
            }
        }

        private static IEnumerable<string> BuildOnlineCandidatesInternal(string connectionString)
        {
            yield return connectionString;

            MySqlConnectionStringBuilder original;
            try
            {
                original = new MySqlConnectionStringBuilder(connectionString);
            }
            catch
            {
                yield break;
            }

            original.ConnectionTimeout = Math.Max(8u, original.ConnectionTimeout);
            yield return original.ConnectionString;

            var ipv4 = ResolveIpv4(original.Server);
            if (!string.IsNullOrWhiteSpace(ipv4))
            {
                var ipv4Builder = new MySqlConnectionStringBuilder(original.ConnectionString)
                {
                    Server = ipv4
                };
                yield return ipv4Builder.ConnectionString;

                var ipv4Preferred = new MySqlConnectionStringBuilder(ipv4Builder.ConnectionString)
                {
                    SslMode = MySqlSslMode.Preferred
                };
                yield return ipv4Preferred.ConnectionString;
            }

            var preferredBuilder = new MySqlConnectionStringBuilder(original.ConnectionString)
            {
                SslMode = MySqlSslMode.Preferred
            };
            yield return preferredBuilder.ConnectionString;
        }

        private static bool CanOpen(string connectionString)
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string DescribeConnection(string connectionString)
        {
            try
            {
                var builder = new MySqlConnectionStringBuilder(connectionString);
                return string.Format(
                    "Server={0};Port={1};Database={2};User={3};SslMode={4};Timeout={5}",
                    builder.Server,
                    builder.Port,
                    builder.Database,
                    builder.UserID,
                    builder.SslMode,
                    builder.ConnectionTimeout);
            }
            catch
            {
                return "<invalid connection string>";
            }
        }

        private static bool IsOnlineMode()
        {
            var mode = Environment.GetEnvironmentVariable("SMS_DB_MODE");
            if (string.IsNullOrWhiteSpace(mode))
            {
                mode = RuntimeConfiguration.ReadDbMode("Local");
            }

            if (string.IsNullOrWhiteSpace(mode))
            {
                return false;
            }

            mode = mode.Trim();
            return string.Equals(mode, "online", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "hostinger", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "cloud", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(mode, "internet", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildDirectOnlineConnectionString()
        {
            var host = ReadModeAwareSetting("SMS_DB_HOST", "DbHostOnline", "DbHost");
            var portValue = ReadModeAwareSetting("SMS_DB_PORT", "DbPortOnline", "DbPort");
            var database = ReadModeAwareSetting("SMS_DB_NAME", "DbNameOnline", "DbName");
            var user = ReadModeAwareSetting("SMS_DB_USER", "DbUserOnline", "DbUser");
            var password = ReadModeAwareSetting("SMS_DB_PASSWORD", "DbPasswordOnline", "DbPassword");
            var sslModeValue = ReadModeAwareSetting("SMS_DB_SSL_MODE", "DbSslModeOnline", "DbSslMode");
            var sslCaPath = ReadModeAwareSetting("SMS_DB_SSL_CA_PATH", "DbSslCaPathOnline", "DbSslCaPath");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user))
            {
                return null;
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = host.Trim(),
                Database = database.Trim(),
                UserID = user.Trim(),
                Password = SensitiveDataProtector.Unprotect(password),
                AllowPublicKeyRetrieval = true,
                ConnectionTimeout = 15,
                SslMode = MySqlSslMode.Required
            };

            uint port;
            if (uint.TryParse(portValue, out port) && port > 0)
            {
                builder.Port = port;
            }

            MySqlSslMode sslMode;
            if (!string.IsNullOrWhiteSpace(sslModeValue) &&
                Enum.TryParse(sslModeValue.Trim(), true, out sslMode))
            {
                builder.SslMode = sslMode;
            }

            if (!string.IsNullOrWhiteSpace(sslCaPath))
            {
                builder.SslCa = sslCaPath.Trim();
            }

            return builder.ConnectionString;
        }

        private static string ReadModeAwareSetting(string envKey, string primaryAppSettingKey, string fallbackAppSettingKey)
        {
            var envValue = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue.Trim();
            }

            var value = RuntimeConfiguration.ReadAppSetting(primaryAppSettingKey);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            value = RuntimeConfiguration.ReadAppSetting(fallbackAppSettingKey);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ResolveIpv4(string host)
        {
            return NetworkNameResolver.ResolveIpv4(host);
        }

        public int ExecuteNonQuery(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteNonQuery", ex);
                throw;
            }
        }

        public object ExecuteScalar(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteScalar", ex);
                throw;
            }
        }

        public long ExecuteInsert(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return cmd.LastInsertedId;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteInsert", ex);
                throw;
            }
        }

        public DataTable ExecuteDataTable(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new MySqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteDataTable", ex);
                throw;
            }
        }

        public DataSet ExecuteDataSet(string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                using (var cmd = CreateCommand(conn, sql, commandType, parameters))
                using (var da = new MySqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                FileLogger.LogError("DatabaseHelper.ExecuteDataSet", ex);
                throw;
            }
        }

        public void ExecuteInTransaction(Action<MySqlConnection, MySqlTransaction> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        action(conn, tx);
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        FileLogger.LogError("DatabaseHelper.ExecuteInTransaction", ex);
                        throw;
                    }
                }
            }
        }

        private static MySqlCommand CreateCommand(MySqlConnection connection, string sql, CommandType commandType, IEnumerable<MySqlParameter> parameters)
        {
            var cmd = new MySqlCommand(sql, connection);
            cmd.CommandType = commandType;
            cmd.CommandTimeout = 30;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
            }

            return cmd;
        }
    }
}

