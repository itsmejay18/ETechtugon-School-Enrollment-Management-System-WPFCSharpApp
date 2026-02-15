using System;
using System.Configuration;
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

            return cs.ConnectionString;
        }
    }
}
