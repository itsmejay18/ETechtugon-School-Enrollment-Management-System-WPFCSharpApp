using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using School_Management_System.Common;

namespace School_Management_System.DataLayer.Configuration
{
    /// <summary>
    /// Provides centralized access to configured database connection strings.
    /// </summary>
    public static class ConnectionStringProvider
    {
        /// <summary>
        /// Gets the application's default database connection string.
        /// </summary>
        public static string GetDefault()
        {
            return GetByName(AppConstants.ConnectionStringName);
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

            return builder.ConnectionString;
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
