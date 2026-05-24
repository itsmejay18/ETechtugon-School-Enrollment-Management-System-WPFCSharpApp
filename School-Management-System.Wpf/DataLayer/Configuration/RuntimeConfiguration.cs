using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace School_Management_System.DataLayer.Configuration
{
    public static class RuntimeConfiguration
    {
        private static readonly object Sync = new object();
        private static IDictionary<string, string> _fallbackAppSettings;

        public static string ReadAppSetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            var value = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return ReadFallbackAppSetting(key);
        }

        public static string ReadDbMode(string defaultMode)
        {
            var envMode = Environment.GetEnvironmentVariable("SMS_DB_MODE");
            if (!string.IsNullOrWhiteSpace(envMode))
            {
                return envMode;
            }

            var configuredMode = ConfigurationManager.AppSettings["DbMode"];
            if (HasConfiguredProfile(configuredMode, false))
            {
                return configuredMode;
            }

            var fallbackMode = ReadFallbackAppSetting("DbMode");
            if (HasConfiguredProfile(fallbackMode, true))
            {
                return fallbackMode;
            }

            if (!string.IsNullOrWhiteSpace(configuredMode))
            {
                return configuredMode;
            }

            if (!string.IsNullOrWhiteSpace(fallbackMode))
            {
                return fallbackMode;
            }

            return defaultMode;
        }

        private static bool HasConfiguredProfile(string mode, bool fallbackOnly)
        {
            var normalized = NormalizeKnownMode(mode);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var host = ReadProfileSetting(normalized, "Host", fallbackOnly);
            var database = ReadProfileSetting(normalized, "Name", fallbackOnly);
            var user = ReadProfileSetting(normalized, "User", fallbackOnly);

            return !string.IsNullOrWhiteSpace(host) &&
                   !string.IsNullOrWhiteSpace(database) &&
                   !string.IsNullOrWhiteSpace(user);
        }

        private static string ReadProfileSetting(string mode, string settingPart, bool fallbackOnly)
        {
            var key = "Db" + settingPart;
            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                key += "Online";
            }

            if (fallbackOnly)
            {
                return ReadFallbackAppSetting(key);
            }

            return ReadAppSetting(key);
        }

        private static string NormalizeKnownMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return null;
            }

            var normalized = mode.Trim();
            if (string.Equals(normalized, "online", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "hostinger", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "cloud", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "internet", StringComparison.OrdinalIgnoreCase))
            {
                return "Online";
            }

            if (string.Equals(normalized, "local", StringComparison.OrdinalIgnoreCase))
            {
                return "Local";
            }

            if (string.Equals(normalized, "wired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "lan", StringComparison.OrdinalIgnoreCase))
            {
                return "Wired";
            }

            if (string.Equals(normalized, "wireless", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "wifi", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "wlan", StringComparison.OrdinalIgnoreCase))
            {
                return "Wireless";
            }

            return null;
        }

        private static string ReadFallbackAppSetting(string key)
        {
            EnsureFallbackAppSettingsLoaded();

            string value;
            return _fallbackAppSettings != null && _fallbackAppSettings.TryGetValue(key, out value)
                ? value
                : null;
        }

        private static void EnsureFallbackAppSettingsLoaded()
        {
            if (_fallbackAppSettings != null)
            {
                return;
            }

            lock (Sync)
            {
                if (_fallbackAppSettings != null)
                {
                    return;
                }

                var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var path in GetFallbackConfigPaths())
                {
                    LoadAppSettings(path, settings);
                }

                _fallbackAppSettings = settings;
            }
        }

        private static IEnumerable<string> GetFallbackConfigPaths()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var fileName in new[] { "SMSApp.exe.config", "School-Management-System.exe.config" })
            {
                var directPath = Path.Combine(baseDirectory, fileName);
                if (seen.Add(directPath))
                {
                    yield return directPath;
                }
            }

            var current = new DirectoryInfo(baseDirectory);
            while (current != null)
            {
                foreach (var configuration in new[] { "Debug", "Release" })
                {
                    foreach (var fileName in new[] { "SMSApp.exe.config", "School-Management-System.exe.config" })
                    {
                        var siblingPath = Path.Combine(
                            current.FullName,
                            "School-Management-System",
                            "bin",
                            configuration,
                            fileName);

                        if (seen.Add(siblingPath))
                        {
                            yield return siblingPath;
                        }
                    }
                }

                current = current.Parent;
            }
        }

        private static void LoadAppSettings(string path, IDictionary<string, string> settings)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            try
            {
                var document = new XmlDocument();
                document.Load(path);

                var nodes = document.SelectNodes("/configuration/appSettings/add");
                if (nodes == null)
                {
                    return;
                }

                var fileSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (XmlNode node in nodes)
                {
                    var key = node.Attributes == null ? null : node.Attributes["key"];
                    var value = node.Attributes == null ? null : node.Attributes["value"];
                    if (key == null || string.IsNullOrWhiteSpace(key.Value) || value == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(value.Value))
                    {
                        fileSettings[key.Value] = value.Value;
                    }
                }

                foreach (var pair in fileSettings)
                {
                    if (!settings.ContainsKey(pair.Key))
                    {
                        settings[pair.Key] = pair.Value;
                    }
                }

                string mode;
                if (fileSettings.TryGetValue("DbMode", out mode) && HasConfiguredProfile(fileSettings, mode))
                {
                    settings["DbMode"] = mode;
                }
            }
            catch
            {
                // Fallback config files are optional. Ignore malformed legacy copies.
            }
        }

        private static bool HasConfiguredProfile(IDictionary<string, string> settings, string mode)
        {
            var normalized = NormalizeKnownMode(mode);
            if (settings == null || string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var suffix = string.Equals(normalized, "Online", StringComparison.OrdinalIgnoreCase) ? "Online" : string.Empty;

            string host;
            string database;
            string user;
            settings.TryGetValue("DbHost" + suffix, out host);
            settings.TryGetValue("DbName" + suffix, out database);
            settings.TryGetValue("DbUser" + suffix, out user);

            return !string.IsNullOrWhiteSpace(host) &&
                   !string.IsNullOrWhiteSpace(database) &&
                   !string.IsNullOrWhiteSpace(user);
        }
    }
}
