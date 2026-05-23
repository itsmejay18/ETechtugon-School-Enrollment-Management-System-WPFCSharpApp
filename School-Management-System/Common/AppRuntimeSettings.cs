using System;
using System.Collections.Generic;
using System.Configuration;

namespace School_Management_System.Common
{
    public sealed class DemoQuickLoginAccount
    {
        public DemoQuickLoginAccount(string label, string username, string password)
        {
            Label = label ?? string.Empty;
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
        }

        public string Label { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
    }

    public static class AppRuntimeSettings
    {
        public static bool EnableDemoQuickLogin
        {
            get { return ReadBoolean("SMS_ENABLE_DEMO_LOGINS", "EnableDemoQuickLogin", false); }
        }

        public static IReadOnlyList<DemoQuickLoginAccount> GetDemoQuickLoginAccounts()
        {
            if (!EnableDemoQuickLogin)
            {
                return new DemoQuickLoginAccount[0];
            }

            var raw = ReadValue("SMS_DEMO_QUICK_LOGINS", "DemoQuickLoginAccounts");
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new DemoQuickLoginAccount[0];
            }

            var accounts = new List<DemoQuickLoginAccount>();
            var entries = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < entries.Length; i++)
            {
                var parts = entries[i].Split(new[] { '|' }, 3);
                if (parts.Length != 3)
                {
                    continue;
                }

                var label = parts[0].Trim();
                var username = parts[1].Trim();
                var password = parts[2];
                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(username))
                {
                    continue;
                }

                accounts.Add(new DemoQuickLoginAccount(label, username, password));
            }

            return accounts.AsReadOnly();
        }

        private static bool ReadBoolean(string environmentVariable, string appSettingKey, bool fallback)
        {
            var value = ReadValue(environmentVariable, appSettingKey);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            value = value.Trim();
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "on", StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadValue(string environmentVariable, string appSettingKey)
        {
            var envValue = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                return envValue;
            }

            return ConfigurationManager.AppSettings[appSettingKey];
        }
    }
}
