using System;
using System.Collections.Generic;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class SystemSettingService
    {
        private readonly ISystemSettingData _data;

        public SystemSettingService(ISystemSettingData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public string Get(string key)
        {
            var setting = _data.Get(key);
            return setting == null ? null : setting.SettingValue;
        }

        public IDictionary<string, string> GetAll()
        {
            return _data.GetAll();
        }

        public void Set(string key, string value)
        {
            _data.Set(key, value);
        }

        public (int? AcademicYearId, int? SemesterId) GetActiveTerm()
        {
            int? ay = ParseNullableInt(Get(AppConstants.SettingKeys.CurrentAcademicYearId));
            int? sem = ParseNullableInt(Get(AppConstants.SettingKeys.CurrentSemesterId));
            return (ay, sem);
        }

        public void SetActiveTerm(int academicYearId, int semesterId)
        {
            _data.Set(AppConstants.SettingKeys.CurrentAcademicYearId, academicYearId.ToString());
            _data.Set(AppConstants.SettingKeys.CurrentSemesterId, semesterId.ToString());
        }

        public string GetDbConnectionMode()
        {
            return NormalizeDbMode(Get(AppConstants.SettingKeys.DbConnectionMode));
        }

        public void SetDbConnectionMode(string mode)
        {
            _data.Set(AppConstants.SettingKeys.DbConnectionMode, NormalizeDbMode(mode));
        }

        public (string Host, string Port, string Database, string Username, string Password) GetDbProfile(string mode)
        {
            var normalizedMode = NormalizeDbMode(mode);
            string hostKey;
            string portKey;
            string dbKey;
            string userKey;
            string passwordKey;
            ResolveProfileKeys(normalizedMode, out hostKey, out portKey, out dbKey, out userKey, out passwordKey);

            var host = Get(hostKey);
            var port = Get(portKey);
            var db = Get(dbKey);
            var user = Get(userKey);
            var password = Get(passwordKey);

            // Backward compatibility with old "Network" keys.
            if (string.Equals(normalizedMode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(host)) host = Get(AppConstants.SettingKeys.DbHostNetwork);
                if (string.IsNullOrWhiteSpace(port)) port = Get(AppConstants.SettingKeys.DbPortNetwork);
                if (string.IsNullOrWhiteSpace(db)) db = Get(AppConstants.SettingKeys.DbNameNetwork);
            }

            return (host, port, db, user, password);
        }

        public void SetDbProfile(string mode, string host, string port, string database, string username, string password)
        {
            var normalizedMode = NormalizeDbMode(mode);
            string hostKey;
            string portKey;
            string dbKey;
            string userKey;
            string passwordKey;
            ResolveProfileKeys(normalizedMode, out hostKey, out portKey, out dbKey, out userKey, out passwordKey);

            _data.Set(hostKey, host ?? string.Empty);
            _data.Set(portKey, port ?? string.Empty);
            _data.Set(dbKey, database ?? string.Empty);
            _data.Set(userKey, username ?? string.Empty);
            _data.Set(passwordKey, password ?? string.Empty);

            if (string.Equals(normalizedMode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                // Keep legacy keys updated for older builds/clients.
                _data.Set(AppConstants.SettingKeys.DbHostNetwork, host ?? string.Empty);
                _data.Set(AppConstants.SettingKeys.DbPortNetwork, port ?? string.Empty);
                _data.Set(AppConstants.SettingKeys.DbNameNetwork, database ?? string.Empty);
            }
        }

        private static void ResolveProfileKeys(
            string mode,
            out string hostKey,
            out string portKey,
            out string dbKey,
            out string userKey,
            out string passwordKey)
        {
            if (string.Equals(mode, "Wireless", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostWireless;
                portKey = AppConstants.SettingKeys.DbPortWireless;
                dbKey = AppConstants.SettingKeys.DbNameWireless;
                userKey = AppConstants.SettingKeys.DbUserWireless;
                passwordKey = AppConstants.SettingKeys.DbPasswordWireless;
                return;
            }

            if (string.Equals(mode, "Wired", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostWired;
                portKey = AppConstants.SettingKeys.DbPortWired;
                dbKey = AppConstants.SettingKeys.DbNameWired;
                userKey = AppConstants.SettingKeys.DbUserWired;
                passwordKey = AppConstants.SettingKeys.DbPasswordWired;
                return;
            }

            if (string.Equals(mode, "Online", StringComparison.OrdinalIgnoreCase))
            {
                hostKey = AppConstants.SettingKeys.DbHostOnline;
                portKey = AppConstants.SettingKeys.DbPortOnline;
                dbKey = AppConstants.SettingKeys.DbNameOnline;
                userKey = AppConstants.SettingKeys.DbUserOnline;
                passwordKey = AppConstants.SettingKeys.DbPasswordOnline;
                return;
            }

            hostKey = AppConstants.SettingKeys.DbHostLocal;
            portKey = AppConstants.SettingKeys.DbPortLocal;
            dbKey = AppConstants.SettingKeys.DbNameLocal;
            userKey = AppConstants.SettingKeys.DbUserLocal;
            passwordKey = AppConstants.SettingKeys.DbPasswordLocal;
        }

        private static string NormalizeDbMode(string mode)
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

        private static int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }
    }
}
