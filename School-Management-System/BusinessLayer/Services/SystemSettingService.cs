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
            return Get(AppConstants.SettingKeys.DbConnectionMode);
        }

        public void SetDbConnectionMode(string mode)
        {
            _data.Set(AppConstants.SettingKeys.DbConnectionMode, mode ?? string.Empty);
        }

        public (string Host, string Port, string Database) GetDbProfile(string mode)
        {
            var isNetwork = string.Equals(mode, "Network", StringComparison.OrdinalIgnoreCase);
            var hostKey = isNetwork ? AppConstants.SettingKeys.DbHostNetwork : AppConstants.SettingKeys.DbHostLocal;
            var portKey = isNetwork ? AppConstants.SettingKeys.DbPortNetwork : AppConstants.SettingKeys.DbPortLocal;
            var dbKey = isNetwork ? AppConstants.SettingKeys.DbNameNetwork : AppConstants.SettingKeys.DbNameLocal;

            return (Get(hostKey), Get(portKey), Get(dbKey));
        }

        public void SetDbProfile(string mode, string host, string port, string database)
        {
            var isNetwork = string.Equals(mode, "Network", StringComparison.OrdinalIgnoreCase);
            var hostKey = isNetwork ? AppConstants.SettingKeys.DbHostNetwork : AppConstants.SettingKeys.DbHostLocal;
            var portKey = isNetwork ? AppConstants.SettingKeys.DbPortNetwork : AppConstants.SettingKeys.DbPortLocal;
            var dbKey = isNetwork ? AppConstants.SettingKeys.DbNameNetwork : AppConstants.SettingKeys.DbNameLocal;

            _data.Set(hostKey, host ?? string.Empty);
            _data.Set(portKey, port ?? string.Empty);
            _data.Set(dbKey, database ?? string.Empty);
        }

        private static int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }
    }
}
