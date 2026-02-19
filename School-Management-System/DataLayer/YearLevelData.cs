using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class YearLevelData : IYearLevelData
    {
        private readonly DatabaseHelper _db;

        public YearLevelData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT YearLevelId, Name, SortOrder, IsActive
FROM YearLevel
WHERE IsActive = 1
ORDER BY SortOrder, Name;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();
            const string sql = @"
SELECT YearLevelId, Name, SortOrder, IsActive
FROM YearLevel
WHERE IsActive = 1
  AND Name LIKE @Q
ORDER BY SortOrder, Name;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public int Insert(YearLevel yearLevel)
        {
            Guard.NotNull(yearLevel, nameof(yearLevel));
            const string sql = @"
INSERT INTO YearLevel (Name, SortOrder, IsActive)
VALUES (@Name, @SortOrder, 1);";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Name", (object)yearLevel.Name ?? DBNull.Value),
                    new MySqlParameter("@SortOrder", yearLevel.SortOrder)
                });

            return Convert.ToInt32(id);
        }

        public void Update(YearLevel yearLevel)
        {
            Guard.NotNull(yearLevel, nameof(yearLevel));
            const string sql = @"
UPDATE YearLevel
SET Name = @Name,
    SortOrder = @SortOrder
WHERE YearLevelId = @YearLevelId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@YearLevelId", yearLevel.YearLevelId),
                    new MySqlParameter("@Name", (object)yearLevel.Name ?? DBNull.Value),
                    new MySqlParameter("@SortOrder", yearLevel.SortOrder)
                });
        }

        public void Delete(int yearLevelId)
        {
            const string sql = @"UPDATE YearLevel SET IsActive = 0 WHERE YearLevelId = @YearLevelId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@YearLevelId", yearLevelId) });
        }
    }
}
