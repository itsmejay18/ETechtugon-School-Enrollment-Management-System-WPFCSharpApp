using System;
using System.Data;
using MySqlConnector;
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
SELECT
    d.DepartmentId,
    d.CollegeId,
    c.CollegeCode,
    c.CollegeName,
    d.DepartmentCode,
    d.DepartmentName,
    d.DepartmentHead,
    d.Description,
    d.CreatedAt
FROM department d
LEFT JOIN college c ON c.CollegeId = d.CollegeId
WHERE d.IsActive = 1
ORDER BY d.DepartmentName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetLookupActive()
        {
            const string sql = @"
SELECT
    d.DepartmentId,
    d.CollegeId,
    c.CollegeCode,
    d.DepartmentCode,
    d.DepartmentName
FROM department d
LEFT JOIN college c ON c.CollegeId = d.CollegeId
WHERE d.IsActive = 1
ORDER BY d.DepartmentName;";
            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    d.DepartmentId,
    d.CollegeId,
    c.CollegeCode,
    c.CollegeName,
    d.DepartmentCode,
    d.DepartmentName,
    d.DepartmentHead,
    d.Description,
    d.CreatedAt
FROM department d
LEFT JOIN college c ON c.CollegeId = d.CollegeId
WHERE d.IsActive = 1
  AND (d.DepartmentCode LIKE @Q OR d.DepartmentName LIKE @Q OR d.DepartmentHead LIKE @Q OR c.CollegeCode LIKE @Q OR c.CollegeName LIKE @Q)
ORDER BY d.DepartmentName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public int Insert(Department department)
        {
            Guard.NotNull(department, nameof(department));

            const string sql = @"
INSERT INTO department (CollegeId, DepartmentCode, DepartmentName, DepartmentHead, Description, IsActive, CreatedAt)
VALUES (@CollegeId, @DepartmentCode, @DepartmentName, @DepartmentHead, @Description, 1, UTC_TIMESTAMP());";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CollegeId", (object)department.CollegeId ?? DBNull.Value),
                    new MySqlParameter("@DepartmentCode", (object)department.DepartmentCode ?? DBNull.Value),
                    new MySqlParameter("@DepartmentName", (object)department.DepartmentName ?? DBNull.Value),
                    new MySqlParameter("@DepartmentHead", (object)department.DepartmentHead ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)department.Description ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Department department)
        {
            Guard.NotNull(department, nameof(department));

            const string sql = @"
UPDATE department
SET
    CollegeId = @CollegeId,
    DepartmentCode = @DepartmentCode,
    DepartmentName = @DepartmentName,
    DepartmentHead = @DepartmentHead,
    Description = @Description,
    UpdatedAt = UTC_TIMESTAMP()
WHERE DepartmentId = @DepartmentId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@DepartmentId", department.DepartmentId),
                    new MySqlParameter("@CollegeId", (object)department.CollegeId ?? DBNull.Value),
                    new MySqlParameter("@DepartmentCode", (object)department.DepartmentCode ?? DBNull.Value),
                    new MySqlParameter("@DepartmentName", (object)department.DepartmentName ?? DBNull.Value),
                    new MySqlParameter("@DepartmentHead", (object)department.DepartmentHead ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)department.Description ?? DBNull.Value)
                });
        }

        public void Delete(int departmentId)
        {
            const string sql = @"UPDATE department SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE DepartmentId = @DepartmentId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@DepartmentId", departmentId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM department WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}

