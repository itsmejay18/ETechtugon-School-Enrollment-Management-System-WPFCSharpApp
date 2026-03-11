using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class CourseData : ICourseData
    {
        private readonly DatabaseHelper _db;

        public CourseData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    CourseId,
    CourseCode,
    CourseName,
    Description,
    DepartmentId,
    CreatedAt
FROM course
WHERE IsActive = 1
ORDER BY CourseName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetLookupActive()
        {
            const string sql = @"
SELECT CourseId, CourseCode, CourseName
FROM course
WHERE IsActive = 1
ORDER BY CourseName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    CourseId,
    CourseCode,
    CourseName,
    Description,
    DepartmentId,
    CreatedAt
FROM course
WHERE IsActive = 1
  AND (CourseCode LIKE @Q OR CourseName LIKE @Q)
ORDER BY CourseName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Q", "%" + query + "%")
                });
        }

        public int Insert(Course course)
        {
            Guard.NotNull(course, nameof(course));

            const string sql = @"
INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt)
VALUES (@CourseCode, @CourseName, @Description, @DepartmentId, 1, UTC_TIMESTAMP());";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CourseCode", (object)course.CourseCode ?? DBNull.Value),
                    new MySqlParameter("@CourseName", (object)course.CourseName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)course.Description ?? DBNull.Value),
                    new MySqlParameter("@DepartmentId", (object)course.DepartmentId ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Course course)
        {
            Guard.NotNull(course, nameof(course));

            const string sql = @"
UPDATE course
SET
    CourseCode = @CourseCode,
    CourseName = @CourseName,
    Description = @Description,
    DepartmentId = @DepartmentId,
    UpdatedAt = UTC_TIMESTAMP()
WHERE CourseId = @CourseId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CourseId", course.CourseId),
                    new MySqlParameter("@CourseCode", (object)course.CourseCode ?? DBNull.Value),
                    new MySqlParameter("@CourseName", (object)course.CourseName ?? DBNull.Value),
                    new MySqlParameter("@Description", (object)course.Description ?? DBNull.Value),
                    new MySqlParameter("@DepartmentId", (object)course.DepartmentId ?? DBNull.Value)
                });
        }

        public void Delete(int courseId)
        {
            const string sql = @"UPDATE course SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE CourseId = @CourseId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@CourseId", courseId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM course WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
