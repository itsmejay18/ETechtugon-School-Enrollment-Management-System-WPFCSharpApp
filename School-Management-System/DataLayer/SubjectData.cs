using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class SubjectData : ISubjectData
    {
        private readonly DatabaseHelper _db;

        public SubjectData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    s.CourseId,
    c.CourseCode,
    s.CreatedAt
FROM subject s
LEFT JOIN course c ON c.CourseId = s.CourseId
WHERE s.IsActive = 1
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetByCourse(int courseId)
        {
            const string sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    s.CourseId,
    c.CourseCode
FROM subject s
LEFT JOIN course c ON c.CourseId = s.CourseId
WHERE s.IsActive = 1
  AND (s.CourseId = @CourseId OR s.CourseId IS NULL)
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@CourseId", courseId) });
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    s.CourseId,
    c.CourseCode,
    s.CreatedAt
FROM subject s
LEFT JOIN course c ON c.CourseId = s.CourseId
WHERE s.IsActive = 1
  AND (s.SubjectCode LIKE @Q OR s.SubjectName LIKE @Q OR c.CourseCode LIKE @Q)
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public int Insert(Subject subject)
        {
            Guard.NotNull(subject, nameof(subject));

            const string sql = @"
INSERT INTO subject (SubjectCode, SubjectName, Units, CourseId, IsActive, CreatedAt)
VALUES (@SubjectCode, @SubjectName, @Units, @CourseId, 1, UTC_TIMESTAMP());";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SubjectCode", (object)subject.SubjectCode ?? DBNull.Value),
                    new MySqlParameter("@SubjectName", (object)subject.SubjectName ?? DBNull.Value),
                    new MySqlParameter("@Units", subject.Units),
                    new MySqlParameter("@CourseId", (object)subject.CourseId ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Subject subject)
        {
            Guard.NotNull(subject, nameof(subject));

            const string sql = @"
UPDATE subject
SET
    SubjectCode = @SubjectCode,
    SubjectName = @SubjectName,
    Units = @Units,
    CourseId = @CourseId,
    UpdatedAt = UTC_TIMESTAMP()
WHERE SubjectId = @SubjectId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SubjectId", subject.SubjectId),
                    new MySqlParameter("@SubjectCode", (object)subject.SubjectCode ?? DBNull.Value),
                    new MySqlParameter("@SubjectName", (object)subject.SubjectName ?? DBNull.Value),
                    new MySqlParameter("@Units", subject.Units),
                    new MySqlParameter("@CourseId", (object)subject.CourseId ?? DBNull.Value)
                });
        }

        public void Delete(int subjectId)
        {
            const string sql = @"UPDATE subject SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE SubjectId = @SubjectId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@SubjectId", subjectId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM subject WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
