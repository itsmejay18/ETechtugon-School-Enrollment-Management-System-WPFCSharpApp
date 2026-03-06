using System;
using System.Collections.Generic;
using System.Configuration;
using MySql.Data.MySqlClient;
using School_Management_System.Common;

namespace School_Management_System.DataLayer.Configuration
{
    /// <summary>
    /// Provides centralized access to configured database connection strings.
    /// Supports process-level overrides (command line/env), App.config fallback,
    /// and database-driven connection profiles from SystemSetting.
    /// </summary>
    public static class ConnectionStringProvider
    {
        private static readonly object DbSettingsSync = new object();
        private static IDictionary<string, string> _cachedDbSettings;
        private static bool _dbSettingsLoaded;

        /// <summary>
        /// Gets the application's default database connection string.
        /// </summary>
        public static string GetDefault()
        {
            return GetByName(AppConstants.ConnectionStringName);
        }

        /// <summary>
        /// Clears cached SystemSetting connection profile values.
        /// Call this after updating DB profile settings in the database.
        /// </summary>
        public static void ResetDatabaseProfileCache()
        {
            lock (DbSettingsSync)
            {
                _cachedDbSettings = null;
                _dbSettingsLoaded = false;
            }
        }

        /// <summary>
        /// Gets a connection string by name from App.config.
        /// </summary>
        public static string GetByName(string connectionStringName)
        {
            Guard.NotNullOrWhiteSpace(connectionStringName, nameof(connectionStringName));

            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Missing connection string '" + connectionStringName + "' in App.config.");
            }

            return ApplyOverrides(cs.ConnectionString);
        }

        private static string ApplyOverrides(string baseConnectionString)
        {
            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                return baseConnectionString;
            }

            var builder = new MySqlConnectionStringBuilder(baseConnectionString);

            // First-level direct override from command line/env/App.config.
            ApplyDirectOverrides(builder);

            // Second-level profile override from SystemSetting table.
            ApplyDatabaseProfileOverrides(builder);

            return builder.ConnectionString;
        }

        private static void ApplyDirectOverrides(MySqlConnectionStringBuilder builder)
        {
            var host = ReadOverride("SMS_DB_HOST", "DbHost");
            if (!string.IsNullOrWhiteSpace(host))
            {
                builder.Server = host.Trim();
            }

            var portValue = ReadOverride("SMS_DB_PORT", "DbPort");
            uint port;
            if (!string.IsNullOrWhiteSpace(portValue) && uint.TryParse(portValue, out port) && port > 0)
            {
                builder.Port = port;
            }

            var database = ReadOverride("SMS_DB_NAME", "DbName");
            if (!string.IsNullOrWhiteSpace(database))
            {
                builder.Database = database.Trim();
            }

            var user = ReadOverride("SMS_DB_USER", "DbUser");
            if (!string.IsNullOrWhiteSpace(user))
            {
                builder.UserID = user.Trim();
            }

            var password = ReadOverride("SMS_DB_PASSWORD", "DbPassword");
            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = password;
            }
        }

        private static void ApplyDatabaseProfileOverrides(MySqlConnectionStringBuilder builder)
        {
            var dbSettings = ReadDbSettings(builder.ConnectionString);
            if (dbSettings == null || dbSettings.Count == 0)
            {
                return;
            }

            var preferredMode = ReadOverride("SMS_DB_MODE", "DbMode");
            if (string.IsNullOrWhiteSpace(preferredMode))
            {
                preferredMode = ReadSetting(dbSettings, AppConstants.SettingKeys.DbConnectionMode);
            }

            var mode = NormalizeMode(preferredMode);

            string hostKey;
            string portKey;
            string dbNameKey;
            string userKey;
            string passwordKey;
            string hostLegacyKey;
            string portLegacyKey;
            string dbLegacyKey;
            ResolveProfileKeys(
                mode,
                out hostKey,
                out portKey,
                out dbNameKey,
                out userKey,
                out passwordKey,
                out hostLegacyKey,
                out portLegacyKey,
                out dbLegacyKey);

            var host = ReadSetting(dbSettings, hostKey);
            var portValue = ReadSetting(dbSettings, portKey);
            var dbName = ReadSetting(dbSettings, dbNameKey);
            var user = ReadSetting(dbSettings, userKey);
            var password = ReadSetting(dbSettings, passwordKey);

            if (string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(hostLegacyKey))
            {
                host = ReadSetting(dbSettings, hostLegacyKey);
            }

            if (string.IsNullOrWhiteSpace(portValue) && !string.IsNullOrWhiteSpace(portLegacyKey))
            {
                portValue = ReadSetting(dbSettings, portLegacyKey);
            }

            if (string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(dbLegacyKey))
            {
                dbName = ReadSetting(dbSettings, dbLegacyKey);
            }

            if (!string.IsNullOrWhiteSpace(host))
            {
                builder.Server = host.Trim();
            }

            uint port;
            if (!string.IsNullOrWhiteSpace(portValue) && uint.TryParse(portValue, out port) && port > 0)
            {
                builder.Port = port;
            }

            if (!string.IsNullOrWhiteSpace(dbName))
            {
                builder.Database = dbName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(user))
            {
                builder.UserID = user.Trim();
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = password;
            }
        }

        private static IDictionary<string, string> ReadDbSettings(string bootstrapConnectionString)
        {
            lock (DbSettingsSync)
            {
                if (_dbSettingsLoaded)
                {
                    return _cachedDbSettings;
                }
            }

            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using (var conn = new MySqlConnection(bootstrapConnectionString))
                {
                    conn.Open();

                    const string sql = @"
SELECT SettingKey, SettingValue
FROM SystemSetting
WHERE SettingKey = @ModeKey
   OR SettingKey LIKE 'Db%.%';";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ModeKey", AppConstants.SettingKeys.DbConnectionMode);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var key = reader.IsDBNull(0) ? null : reader.GetString(0);
                                if (string.IsNullOrWhiteSpace(key))
                                {
                                    continue;
                                }

                                var value = reader.IsDBNull(1) ? null : reader.GetString(1);
                                settings[key] = value;
                            }
                        }
                    }
                }
            }
            catch
            {
                // If bootstrap DB isn't ready yet, keep fallback behavior from app/env values.
            }

            lock (DbSettingsSync)
            {
                _cachedDbSettings = settings;
                _dbSettingsLoaded = true;
            }

            return settings;
        }

        private static string NormalizeMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Local";
            }

            var normalized = mode.Trim();
            if (string.Equals(normalized, "wireless", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "wifi", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "wlan", StringComparison.OrdinalIgnoreCase))
            {
                return "Wireless";
            }

            if (string.Equals(normalized, "wired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "lan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "ip", StringComparison.OrdinalIgnoreCase))
            {
                return "Wired";
            }

            if (string.Equals(normalized, "online", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "hostinger", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "cloud", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "internet", StringComparison.OrdinalIgnoreCase))
            {
                return "Online";
            }

            return "Local";
        }

        private static void ResolveProfileKeys(
            string mode,
            out string hostKey,
            out string portKey,
            out string dbNameKey,
            out string userKey,
            out string passwordKey,
            out string hostLegacyKey,
            out string portLegacyKey,
            out string dbLegacyKey)
        {
            hostLegacyKey = null;
            portLegacyKey = null;
            dbLegacyKey = null;

            if (string.Equals(mode, "Wireless", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostWireless;
                portKey = AppConstants.SettingKeys.DbPortWireless;
                dbNameKey = AppConstants.SettingKeys.DbNameWireless;
                userKey = AppConstants.SettingKeys.DbUserWireless;
                passwordKey = AppConstants.SettingKeys.DbPasswordWireless;
                return;
            }

            if (string.Equals(mode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostWired;
                portKey = AppConstants.SettingKeys.DbPortWired;
                dbNameKey = AppConstants.SettingKeys.DbNameWired;
                userKey = AppConstants.SettingKeys.DbUserWired;
                passwordKey = AppConstants.SettingKeys.DbPasswordWired;
                hostLegacyKey = AppConstants.SettingKeys.DbHostNetwork;
                portLegacyKey = AppConstants.SettingKeys.DbPortNetwork;
                dbLegacyKey = AppConstants.SettingKeys.DbNameNetwork;
                return;
            }

            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostOnline;
                portKey = AppConstants.SettingKeys.DbPortOnline;
                dbNameKey = AppConstants.SettingKeys.DbNameOnline;
                userKey = AppConstants.SettingKeys.DbUserOnline;
                passwordKey = AppConstants.SettingKeys.DbPasswordOnline;
                return;
            }

            hostKey = AppConstants.SettingKeys.DbHostLocal;
            portKey = AppConstants.SettingKeys.DbPortLocal;
            dbNameKey = AppConstants.SettingKeys.DbNameLocal;
            userKey = AppConstants.SettingKeys.DbUserLocal;
            passwordKey = AppConstants.SettingKeys.DbPasswordLocal;
        }

        private static string ReadSetting(IDictionary<string, string> settings, string key)
        {
            if (settings == null || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            string value;
            return settings.TryGetValue(key, out value) ? value : null;
        }

        private static string ReadOverride(string envVarName, string appSettingKey)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }

            var appValue = ConfigurationManager.AppSettings[appSettingKey];
            return string.IsNullOrWhiteSpace(appValue) ? null : appValue;
        }
    }
}
