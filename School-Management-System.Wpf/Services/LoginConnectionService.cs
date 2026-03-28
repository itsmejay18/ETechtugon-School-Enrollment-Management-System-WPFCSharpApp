using System;
using System.Collections.Generic;
using System.Configuration;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Models;

namespace School_Management_System.Wpf.Services
{
    public sealed class LoginConnectionService
    {
        public IReadOnlyList<ConnectionProfile> GetProfiles()
        {
            var profiles = new List<ConnectionProfile>
            {
                BuildLocalProfile(),
                BuildWiredProfile(),
                BuildWirelessProfile(),
                BuildOnlineProfile()
            };

            var currentMode = ConnectionModeHelper.GetCurrentMode("Online");
            for (var i = 0; i < profiles.Count; i++)
            {
                if (string.Equals(profiles[i].Mode, currentMode, StringComparison.OrdinalIgnoreCase))
                {
                    ApplyEnvironmentOverrides(profiles[i]);
                    break;
                }
            }

            return profiles;
        }

        public string GetCurrentMode()
        {
            return ConnectionModeHelper.GetCurrentMode("Online");
        }

        public ConnectionProbeResult ApplyAndTest(ConnectionProfile profile)
        {
            if (profile == null)
            {
                return new ConnectionProbeResult(false, "Connection not selected", "Choose a database profile before signing in.");
            }

            ApplyRuntimeSelection(profile);

            string errorMessage;
            var database = DatabaseHelper.FromConfig();
            if (database.TestConnection(out errorMessage))
            {
                return new ConnectionProbeResult(true, "Connection ready", BuildSuccessSummary(profile));
            }

            return new ConnectionProbeResult(false, "Connection unavailable", BuildFailureSummary(profile, errorMessage));
        }

        public bool TryLogin(ConnectionProfile profile, string username, string password, out User user, out string statusMessage)
        {
            user = null;
            statusMessage = null;

            if (profile == null)
            {
                statusMessage = "Select a database connection profile first.";
                return false;
            }

            ApplyRuntimeSelection(profile);

            string connectionError;
            var database = DatabaseHelper.FromConfig();
            if (!database.TestConnection(out connectionError))
            {
                statusMessage = BuildFailureSummary(profile, connectionError);
                return false;
            }

            SchemaMigrationRunner.EnsureCurrent(database);

            var activityLogService = new ActivityLogService(new ActivityLogData(database));
            var authService = new AuthService(new UserData(database), activityLogService);
            string authError;
            if (!authService.TryLogin(username, password, out user, out authError))
            {
                statusMessage = string.IsNullOrWhiteSpace(authError)
                    ? "Login failed. Check your username and password."
                    : authError;
                return false;
            }

            statusMessage = "Login successful.";
            return true;
        }

        private static ConnectionProfile BuildLocalProfile()
        {
            return new ConnectionProfile(
                "Local",
                "Local",
                "MySQL on this computer",
                ReadAppSetting("DbHost"),
                ReadAppSetting("DbPort", "3306"),
                ReadAppSetting("DbName", "schoolmanagementsystem"),
                ReadAppSetting("DbUser"),
                ReadAppSetting("DbPassword"));
        }

        private static ConnectionProfile BuildWiredProfile()
        {
            return new ConnectionProfile(
                "Wired",
                "Network",
                "LAN database server",
                ReadAppSetting("DbHostWired", ReadAppSetting("DbHostNetwork", ReadAppSetting("DbHost"))),
                ReadAppSetting("DbPortWired", ReadAppSetting("DbPortNetwork", ReadAppSetting("DbPort", "3306"))),
                ReadAppSetting("DbNameWired", ReadAppSetting("DbNameNetwork", ReadAppSetting("DbName", "schoolmanagementsystem"))),
                ReadAppSetting("DbUserWired", ReadAppSetting("DbUser")),
                ReadAppSetting("DbPasswordWired", ReadAppSetting("DbPassword")));
        }

        private static ConnectionProfile BuildWirelessProfile()
        {
            return new ConnectionProfile(
                "Wireless",
                "Wi-Fi",
                "Wireless campus server",
                ReadAppSetting("DbHostWireless", ReadAppSetting("DbHostWired", ReadAppSetting("DbHostNetwork", ReadAppSetting("DbHost")))),
                ReadAppSetting("DbPortWireless", ReadAppSetting("DbPortWired", ReadAppSetting("DbPortNetwork", ReadAppSetting("DbPort", "3306")))),
                ReadAppSetting("DbNameWireless", ReadAppSetting("DbNameWired", ReadAppSetting("DbNameNetwork", ReadAppSetting("DbName", "schoolmanagementsystem")))),
                ReadAppSetting("DbUserWireless", ReadAppSetting("DbUserWired", ReadAppSetting("DbUser"))),
                ReadAppSetting("DbPasswordWireless", ReadAppSetting("DbPasswordWired", ReadAppSetting("DbPassword"))));
        }

        private static ConnectionProfile BuildOnlineProfile()
        {
            return new ConnectionProfile(
                "Online",
                "Online",
                "Hosted internet database",
                ReadAppSetting("DbHostOnline", ReadAppSetting("DbHost")),
                ReadAppSetting("DbPortOnline", ReadAppSetting("DbPort", "3306")),
                ReadAppSetting("DbNameOnline", ReadAppSetting("DbName", "schoolmanagementsystem")),
                ReadAppSetting("DbUserOnline", ReadAppSetting("DbUser")),
                ReadAppSetting("DbPasswordOnline", ReadAppSetting("DbPassword")));
        }

        private static void ApplyEnvironmentOverrides(ConnectionProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            var host = Environment.GetEnvironmentVariable("SMS_DB_HOST");
            var port = Environment.GetEnvironmentVariable("SMS_DB_PORT");
            var database = Environment.GetEnvironmentVariable("SMS_DB_NAME");
            var user = Environment.GetEnvironmentVariable("SMS_DB_USER");
            var password = Environment.GetEnvironmentVariable("SMS_DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(host)) profile.Host = host.Trim();
            if (!string.IsNullOrWhiteSpace(port)) profile.Port = port.Trim();
            if (!string.IsNullOrWhiteSpace(database)) profile.Database = database.Trim();
            if (!string.IsNullOrWhiteSpace(user)) profile.Username = user.Trim();
            if (!string.IsNullOrWhiteSpace(password)) profile.Password = password;
        }

        private static void ApplyRuntimeSelection(ConnectionProfile profile)
        {
            ConnectionModeHelper.ApplyRuntimeMode(profile.Mode);
            SetProcessVariable("SMS_DB_HOST", profile.Host);
            SetProcessVariable("SMS_DB_PORT", profile.Port);
            SetProcessVariable("SMS_DB_NAME", profile.Database);
            SetProcessVariable("SMS_DB_USER", profile.Username);
            SetProcessVariable("SMS_DB_PASSWORD", profile.Password);

            if (string.Equals(profile.Mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                SetProcessVariable("SMS_DB_SSL_MODE", ReadAppSetting("DbSslModeOnline", "Required"));
                SetProcessVariable("SMS_DB_SSL_CA_PATH", ReadAppSetting("DbSslCaPathOnline"));
            }
            else
            {
                SetProcessVariable("SMS_DB_SSL_MODE", null);
                SetProcessVariable("SMS_DB_SSL_CA_PATH", null);
            }

            ConnectionStringProvider.ResetDatabaseProfileCache();
        }

        private static void SetProcessVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, string.IsNullOrWhiteSpace(value) ? null : value.Trim(), EnvironmentVariableTarget.Process);
        }

        private static string ReadAppSetting(string key, string fallback = "")
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? (fallback ?? string.Empty) : value.Trim();
        }

        private static string BuildSuccessSummary(ConnectionProfile profile)
        {
            return "Connected using the " + profile.DisplayName + " profile. Server " +
                   profile.Host + ":" + profile.Port + " is ready.";
        }

        private static string BuildFailureSummary(ConnectionProfile profile, string errorMessage)
        {
            var baseMessage = "Unable to connect using the " + profile.DisplayName + " profile.";

            if (string.Equals(profile.Mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                return baseMessage + " Check internet access or hosted server availability, then try Refresh.";
            }

            if (string.Equals(profile.Mode, "Local", StringComparison.OrdinalIgnoreCase))
            {
                return baseMessage + " Make sure the MySQL service is running on this computer.";
            }

            return baseMessage + " Make sure the selected server is reachable on the network.";
        }

        public sealed class ConnectionProfile
        {
            public ConnectionProfile(string mode, string displayName, string caption, string host, string port, string database, string username, string password)
            {
                Mode = mode ?? string.Empty;
                DisplayName = displayName ?? string.Empty;
                Caption = caption ?? string.Empty;
                Host = host ?? string.Empty;
                Port = port ?? string.Empty;
                Database = database ?? string.Empty;
                Username = username ?? string.Empty;
                Password = password ?? string.Empty;
            }

            public string Mode { get; private set; }
            public string DisplayName { get; private set; }
            public string Caption { get; private set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string Database { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public sealed class ConnectionProbeResult
        {
            public ConnectionProbeResult(bool isSuccess, string title, string message)
            {
                IsSuccess = isSuccess;
                Title = title ?? string.Empty;
                Message = message ?? string.Empty;
            }

            public bool IsSuccess { get; private set; }
            public string Title { get; private set; }
            public string Message { get; private set; }
        }
    }
}
