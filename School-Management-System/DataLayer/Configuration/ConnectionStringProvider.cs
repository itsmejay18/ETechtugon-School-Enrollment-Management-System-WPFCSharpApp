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
    /// and database-driven connection profiles from systemsetting.
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

            // Second-level profile override from systemsetting table.
            ApplyDatabaseProfileOverrides(builder);

            return builder.ConnectionString;
        }

        private static void ApplyDirectOverrides(MySqlConnectionStringBuilder builder)
        {
            var mode = NormalizeMode(ReadOverride("SMS_DB_MODE", "DbMode"));

            var host = ReadModeAwareOverride("SMS_DB_HOST", "DbHost", mode);
            if (!string.IsNullOrWhiteSpace(host))
            {
                builder.Server = host.Trim();
            }

            var portValue = ReadModeAwareOverride("SMS_DB_PORT", "DbPort", mode);
            uint port;
            if (!string.IsNullOrWhiteSpace(portValue) && uint.TryParse(portValue, out port) && port > 0)
            {
                builder.Port = port;
            }

            var database = ReadModeAwareOverride("SMS_DB_NAME", "DbName", mode);
            if (!string.IsNullOrWhiteSpace(database))
            {
                builder.Database = database.Trim();
            }

            var user = ReadModeAwareOverride("SMS_DB_USER", "DbUser", mode);
            if (!string.IsNullOrWhiteSpace(user))
            {
                builder.UserID = user.Trim();
            }

            var password = ReadModeAwareOverride("SMS_DB_PASSWORD", "DbPassword", mode);
            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = SensitiveDataProtector.Unprotect(password);
            }

            var sslMode = ReadModeAwareOverride("SMS_DB_SSL_MODE", "DbSslMode", mode);
            ApplySslMode(builder, sslMode, false);

            var sslCaPath = ReadModeAwareOverride("SMS_DB_SSL_CA_PATH", "DbSslCaPath", mode);
            if (!string.IsNullOrWhiteSpace(sslCaPath))
            {
                builder.SslCa = sslCaPath.Trim();
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
            var hasDirectHostOverride = HasModeAwareOverride("SMS_DB_HOST", "DbHost", mode);
            var hasDirectPortOverride = HasModeAwareOverride("SMS_DB_PORT", "DbPort", mode);
            var hasDirectDatabaseOverride = HasModeAwareOverride("SMS_DB_NAME", "DbName", mode);
            var hasDirectUserOverride = HasModeAwareOverride("SMS_DB_USER", "DbUser", mode);
            var hasDirectPasswordOverride = HasModeAwareOverride("SMS_DB_PASSWORD", "DbPassword", mode);
            var hasDirectSslModeOverride = HasModeAwareOverride("SMS_DB_SSL_MODE", "DbSslMode", mode);
            var hasDirectSslCaOverride = HasModeAwareOverride("SMS_DB_SSL_CA_PATH", "DbSslCaPath", mode);

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

            if (!hasDirectHostOverride && !string.IsNullOrWhiteSpace(host))
            {
                builder.Server = host.Trim();
            }

            uint port;
            if (!hasDirectPortOverride && !string.IsNullOrWhiteSpace(portValue) && uint.TryParse(portValue, out port) && port > 0)
            {
                builder.Port = port;
            }

            if (!hasDirectDatabaseOverride && !string.IsNullOrWhiteSpace(dbName))
            {
                builder.Database = dbName.Trim();
            }

            if (!hasDirectUserOverride && !string.IsNullOrWhiteSpace(user))
            {
                builder.UserID = user.Trim();
            }

            if (!hasDirectPasswordOverride && !string.IsNullOrWhiteSpace(password))
            {
                builder.Password = SensitiveDataProtector.Unprotect(password);
            }

            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                var sslMode = ReadSetting(dbSettings, AppConstants.SettingKeys.DbSslModeOnline);
                ApplySslMode(builder, hasDirectSslModeOverride ? null : sslMode, true);

                var sslCaPath = ReadSetting(dbSettings, AppConstants.SettingKeys.DbSslCaPathOnline);
                if (!hasDirectSslCaOverride && !string.IsNullOrWhiteSpace(sslCaPath))
                {
                    builder.SslCa = sslCaPath.Trim();
                }
            }
            else
            {
                // Keep Local/Wired/Wireless permissive defaults unless explicitly overridden.
                ApplySslMode(builder, null, false);
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
FROM `systemsetting`
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

        private static string ReadModeAwareOverride(string envVarName, string appSettingKey, string mode)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }

            var profileAppSettingKey = ResolveProfileAppSettingKey(appSettingKey, mode);
            if (!string.IsNullOrWhiteSpace(profileAppSettingKey))
            {
                var profileAppValue = ConfigurationManager.AppSettings[profileAppSettingKey];
                if (!string.IsNullOrWhiteSpace(profileAppValue))
                {
                    return profileAppValue;
                }
            }

            var appValue = ConfigurationManager.AppSettings[appSettingKey];
            return string.IsNullOrWhiteSpace(appValue) ? null : appValue;
        }

        private static bool HasModeAwareOverride(string envVarName, string appSettingKey, string mode)
        {
            return !string.IsNullOrWhiteSpace(ReadModeAwareOverride(envVarName, appSettingKey, mode));
        }

        private static string ResolveProfileAppSettingKey(string appSettingKey, string mode)
        {
            if (string.IsNullOrWhiteSpace(appSettingKey))
            {
                return null;
            }

            if (!string.Equals(NormalizeMode(mode), "Online", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            switch (appSettingKey)
            {
                case "DbHost":
                    return "DbHostOnline";
                case "DbPort":
                    return "DbPortOnline";
                case "DbName":
                    return "DbNameOnline";
                case "DbUser":
                    return "DbUserOnline";
                case "DbPassword":
                    return "DbPasswordOnline";
                case "DbSslMode":
                    return "DbSslModeOnline";
                case "DbSslCaPath":
                    return "DbSslCaPathOnline";
                default:
                    return null;
            }
        }

        private static void ApplySslMode(MySqlConnectionStringBuilder builder, string sslModeValue, bool enforceMinimum)
        {
            if (builder == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sslModeValue))
            {
                var currentMode = builder.SslMode.ToString();
                if (enforceMinimum &&
                    (string.Equals(currentMode, "None", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(currentMode, "Disabled", StringComparison.OrdinalIgnoreCase)))
                {
                    builder.SslMode = MySqlSslMode.Required;
                }

                return;
            }

            MySqlSslMode parsed;
            if (!Enum.TryParse(sslModeValue.Trim(), true, out parsed))
            {
                parsed = enforceMinimum ? MySqlSslMode.Required : builder.SslMode;
            }

            var parsedMode = parsed.ToString();
            if (enforceMinimum &&
                (string.Equals(parsedMode, "None", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(parsedMode, "Disabled", StringComparison.OrdinalIgnoreCase)))
            {
                parsed = MySqlSslMode.Required;
            }

            builder.SslMode = parsed;
        }
    }
}
