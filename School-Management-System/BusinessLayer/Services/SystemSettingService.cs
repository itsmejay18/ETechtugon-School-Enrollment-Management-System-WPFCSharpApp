using System;
using System.Collections.Generic;
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
            int? ay = ParseNullableInt(Get("CurrentAcademicYearId"));
            int? sem = ParseNullableInt(Get("CurrentSemesterId"));
            return (ay, sem);
        }

        public void SetActiveTerm(int academicYearId, int semesterId)
        {
            _data.Set("CurrentAcademicYearId", academicYearId.ToString());
            _data.Set("CurrentSemesterId", semesterId.ToString());
        }

        private static int? ParseNullableInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }
    }
}
