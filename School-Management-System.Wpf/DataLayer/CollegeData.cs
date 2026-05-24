using System;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class CollegeData : ICollegeData
    {
        private readonly DatabaseHelper _db;

        public CollegeData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    c.CollegeId,
    c.CollegeCode,
    c.CollegeName,
    c.DeanName,
    c.Description,
    COUNT(d.DepartmentId) AS DepartmentCount,
    c.CreatedAt
FROM college c
LEFT JOIN department d ON d.CollegeId = c.CollegeId AND d.IsActive = 1
WHERE c.IsActive = 1
GROUP BY c.CollegeId, c.CollegeCode, c.CollegeName, c.DeanName, c.Description, c.CreatedAt
ORDER BY c.CollegeName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetLookupActive()
        {
            const string sql = @"
SELECT CollegeId, CollegeCode, CollegeName
FROM college
WHERE IsActive = 1
ORDER BY CollegeName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    c.CollegeId,
    c.CollegeCode,
    c.CollegeName,
    c.DeanName,
    c.Description,
    COUNT(d.DepartmentId) AS DepartmentCount,
    c.CreatedAt
FROM college c
LEFT JOIN department d ON d.CollegeId = c.CollegeId AND d.IsActive = 1
WHERE c.IsActive = 1
  AND (c.CollegeCode LIKE @Q OR c.CollegeName LIKE @Q OR c.DeanName LIKE @Q)
GROUP BY c.CollegeId, c.CollegeCode, c.CollegeName, c.DeanName, c.Description, c.CreatedAt
ORDER BY c.CollegeName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public int Insert(College college)
        {
            Guard.NotNull(college, nameof(college));

            const string sql = @"
INSERT INTO college (CollegeCode, CollegeName, DeanName, Description, IsActive, CreatedAt)
VALUES (@CollegeCode, @CollegeName, @DeanName, @Description, 1, UTC_TIMESTAMP());";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CollegeCode", (object)college.CollegeCode ?? DBNull.Value),
                    new MySqlParameter("@CollegeName", (object)college.CollegeName ?? DBNull.Value),
                    new MySqlParameter("@DeanName", (object)college.DeanName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)college.Description ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(College college)
        {
            Guard.NotNull(college, nameof(college));

            const string sql = @"
UPDATE college
SET
    CollegeCode = @CollegeCode,
    CollegeName = @CollegeName,
    DeanName = @DeanName,
    Description = @Description,
    UpdatedAt = UTC_TIMESTAMP()
WHERE CollegeId = @CollegeId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CollegeId", college.CollegeId),
                    new MySqlParameter("@CollegeCode", (object)college.CollegeCode ?? DBNull.Value),
                    new MySqlParameter("@CollegeName", (object)college.CollegeName ?? DBNull.Value),
                    new MySqlParameter("@DeanName", (object)college.DeanName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)college.Description ?? DBNull.Value)
                });
        }

        public void Delete(int collegeId)
        {
            const string sql = @"UPDATE college SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE CollegeId = @CollegeId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@CollegeId", collegeId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM college WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
