using System;

namespace School_Management_System.DataLayer.Configuration
{
    public static class ConnectionModeHelper
    {
        public static string Normalize(string mode, string defaultMode = "Local")
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return NormalizeDefault(defaultMode);
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

            return NormalizeDefault(defaultMode);
        }

        public static string GetCurrentMode(string defaultMode = "Online")
        {
            var mode = Environment.GetEnvironmentVariable("SMS_DB_MODE");
            if (!string.IsNullOrWhiteSpace(mode))
            {
                return Normalize(mode, defaultMode);
            }

            mode = RuntimeConfiguration.ReadDbMode(defaultMode);
            return Normalize(mode, defaultMode);
        }

        public static string GetDisplayName(string mode)
        {
            var normalized = Normalize(mode);
            if (string.Equals(normalized, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                return "Network";
            }

            return normalized;
        }

        public static string GetLoginSummary(string mode)
        {
            var normalized = Normalize(mode, "Online");
            switch (normalized)
            {
                case "Online":
                    return "Hosted server profile. Administrators can update connection settings in Settings > Database.";
                case "Wireless":
                    return "Wi-Fi database profile. Connection settings are managed by administrators in Settings.";
                case "Wired":
                    return "LAN database profile. Connection settings are managed by administrators in Settings.";
                default:
                    return "Local database profile. Connection settings are managed by administrators in Settings.";
            }
        }

        public static void ApplyRuntimeMode(string mode)
        {
            var normalized = Normalize(mode);
            Environment.SetEnvironmentVariable("SMS_DB_MODE", normalized, EnvironmentVariableTarget.Process);
            ConnectionStringProvider.ResetDatabaseProfileCache();
        }

        private static string NormalizeDefault(string defaultMode)
        {
            var fallback = string.IsNullOrWhiteSpace(defaultMode) ? "Local" : defaultMode;
            return NormalizeKnownOrLocal(fallback);
        }

        private static string NormalizeKnownOrLocal(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return "Local";
            }

            var normalized = mode.Trim();
            if (string.Equals(normalized, "Wireless", StringComparison.OrdinalIgnoreCase))
            {
                return "Wireless";
            }

            if (string.Equals(normalized, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                return "Wired";
            }

            if (string.Equals(normalized, "Online", StringComparison.OrdinalIgnoreCase))
            {
                return "Online";
            }

            if (string.Equals(normalized, "Local", StringComparison.OrdinalIgnoreCase))
            {
                return "Local";
            }

            return "Local";
        }
    }
}
