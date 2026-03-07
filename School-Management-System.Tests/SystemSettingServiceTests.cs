using System;
using System.Collections.Generic;
using NUnit.Framework;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.Tests.Services
{
    [TestFixture]
    public sealed class SystemSettingServiceTests
    {
        [Test]
        public void Set_WithSensitiveKey_StoresProtectedValue_AndGetReturnsPlainText()
        {
            var data = new InMemorySystemSettingData();
            var service = new SystemSettingService(data);

            service.Set(AppConstants.SettingKeys.DbPasswordOnline, "S3cret!");

            var storedValue = data.GetRawValue(AppConstants.SettingKeys.DbPasswordOnline);
            Assert.That(storedValue, Is.Not.Null.And.Not.EqualTo("S3cret!"));
            Assert.That(SensitiveDataProtector.IsProtected(storedValue), Is.True);
            Assert.That(service.Get(AppConstants.SettingKeys.DbPasswordOnline), Is.EqualTo("S3cret!"));
        }

        [Test]
        public void Set_WithNonSensitiveKey_StoresPlainValue()
        {
            var data = new InMemorySystemSettingData();
            var service = new SystemSettingService(data);

            service.Set(AppConstants.SettingKeys.DbHostOnline, "db.example.com");

            var storedValue = data.GetRawValue(AppConstants.SettingKeys.DbHostOnline);
            Assert.That(storedValue, Is.EqualTo("db.example.com"));
        }

        private sealed class InMemorySystemSettingData : ISystemSettingData
        {
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public SystemSetting Get(string key)
            {
                string value;
                if (!_values.TryGetValue(key, out value))
                {
                    return null;
                }

                return new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value
                };
            }

            public IDictionary<string, string> GetAll()
            {
                return new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase);
            }

            public void Set(string key, string value)
            {
                _values[key] = value ?? string.Empty;
            }

            public string GetRawValue(string key)
            {
                string value;
                return _values.TryGetValue(key, out value) ? value : null;
            }
        }
    }
}

