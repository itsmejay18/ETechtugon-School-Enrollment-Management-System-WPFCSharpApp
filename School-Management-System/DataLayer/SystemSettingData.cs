using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class SystemSettingData : ISystemSettingData
    {
        private readonly DatabaseHelper _db;

        public SystemSettingData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public SystemSetting Get(string key)
        {
            const string sql = @"SELECT SettingKey, SettingValue FROM `systemsetting` WHERE SettingKey = @Key LIMIT 1;";
            var dt = _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@Key", key) });
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new SystemSetting
            {
                SettingKey = Convert.ToString(row["SettingKey"]),
                SettingValue = Convert.ToString(row["SettingValue"])
            };
        }

        public IDictionary<string, string> GetAll()
        {
            const string sql = @"SELECT SettingKey, SettingValue FROM `systemsetting`;";
            var dt = _db.ExecuteDataTable(sql, CommandType.Text, null);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in dt.Rows)
            {
                if (row == null) continue;
                var key = Convert.ToString(row["SettingKey"]);
                var value = Convert.ToString(row["SettingValue"]);
                if (key != null) dict[key] = value;
            }

            return dict;
        }

        public void Set(string key, string value)
        {
            const string sql = @"
INSERT INTO `systemsetting` (SettingKey, SettingValue)
VALUES (@Key, @Value)
ON DUPLICATE KEY UPDATE SettingValue = @Value;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Key", key),
                    new MySqlParameter("@Value", value ?? string.Empty)
                });
        }
    }
}

