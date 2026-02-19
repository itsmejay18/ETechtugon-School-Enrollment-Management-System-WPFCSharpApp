using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class DepartmentData : IDepartmentData
    {
        private readonly DatabaseHelper _db;

        public DepartmentData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT DepartmentId, DepartmentCode, DepartmentName, Description, CreatedAt
FROM Department
WHERE IsActive = 1
ORDER BY DepartmentName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetLookupActive()
        {
            const string sql = @"SELECT DepartmentId, DepartmentCode, DepartmentName FROM Department WHERE IsActive = 1 ORDER BY DepartmentName;";
            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT DepartmentId, DepartmentCode, DepartmentName, Description, CreatedAt
FROM Department
WHERE IsActive = 1
  AND (DepartmentCode LIKE @Q OR DepartmentName LIKE @Q)
ORDER BY DepartmentName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public int Insert(Department department)
        {
            Guard.NotNull(department, nameof(department));

            const string sql = @"
INSERT INTO Department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt)
VALUES (@DepartmentCode, @DepartmentName, @Description, 1, UTC_TIMESTAMP());";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@DepartmentCode", (object)department.DepartmentCode ?? DBNull.Value),
                    new MySqlParameter("@DepartmentName", (object)department.DepartmentName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)department.Description ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Department department)
        {
            Guard.NotNull(department, nameof(department));

            const string sql = @"
UPDATE Department
SET
    DepartmentCode = @DepartmentCode,
    DepartmentName = @DepartmentName,
    Description = @Description,
    UpdatedAt = UTC_TIMESTAMP()
WHERE DepartmentId = @DepartmentId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@DepartmentId", department.DepartmentId),
                    new MySqlParameter("@DepartmentCode", (object)department.DepartmentCode ?? DBNull.Value),
                    new MySqlParameter("@DepartmentName", (object)department.DepartmentName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)department.Description ?? DBNull.Value)
                });
        }

        public void Delete(int departmentId)
        {
            const string sql = @"UPDATE Department SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE DepartmentId = @DepartmentId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@DepartmentId", departmentId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM Department WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
