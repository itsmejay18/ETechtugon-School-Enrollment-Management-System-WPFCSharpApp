using System;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class SectionData : ISectionData
    {
        private readonly DatabaseHelper _db;

        public SectionData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    s.SectionId,
    s.SectionCode,
    s.SectionName,
    s.CourseId,
    c.CourseCode,
    c.CourseName,
    s.YearLevelId,
    yl.Name AS YearLevel,
    s.AcademicYearId,
    ay.Name AS AcademicYear,
    s.SemesterId,
    sem.Name AS Semester,
    s.Capacity,
    s.CreatedAt
FROM section s
INNER JOIN course c ON c.CourseId = s.CourseId
INNER JOIN yearlevel yl ON yl.YearLevelId = s.YearLevelId
INNER JOIN academicyear ay ON ay.AcademicYearId = s.AcademicYearId
INNER JOIN semester sem ON sem.SemesterId = s.SemesterId
WHERE s.IsActive = 1
ORDER BY s.SectionName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();
            const string sql = @"
SELECT
    s.SectionId,
    s.SectionCode,
    s.SectionName,
    s.CourseId,
    c.CourseCode,
    c.CourseName,
    s.YearLevelId,
    yl.Name AS YearLevel,
    s.AcademicYearId,
    ay.Name AS AcademicYear,
    s.SemesterId,
    sem.Name AS Semester,
    s.Capacity,
    s.CreatedAt
FROM section s
INNER JOIN course c ON c.CourseId = s.CourseId
INNER JOIN yearlevel yl ON yl.YearLevelId = s.YearLevelId
INNER JOIN academicyear ay ON ay.AcademicYearId = s.AcademicYearId
INNER JOIN semester sem ON sem.SemesterId = s.SemesterId
WHERE s.IsActive = 1
  AND (s.SectionCode LIKE @Q OR s.SectionName LIKE @Q OR c.CourseCode LIKE @Q OR c.CourseName LIKE @Q)
ORDER BY s.SectionName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public DataTable Search(string query, int? courseId)
        {
            query = (query ?? string.Empty).Trim();
            const string sql = @"
SELECT
    s.SectionId,
    s.SectionCode,
    s.SectionName,
    s.CourseId,
    c.CourseCode,
    c.CourseName,
    s.YearLevelId,
    yl.Name AS YearLevel,
    s.AcademicYearId,
    ay.Name AS AcademicYear,
    s.SemesterId,
    sem.Name AS Semester,
    s.Capacity,
    s.CreatedAt
FROM section s
INNER JOIN course c ON c.CourseId = s.CourseId
INNER JOIN yearlevel yl ON yl.YearLevelId = s.YearLevelId
INNER JOIN academicyear ay ON ay.AcademicYearId = s.AcademicYearId
INNER JOIN semester sem ON sem.SemesterId = s.SemesterId
WHERE s.IsActive = 1
  AND (@CourseId IS NULL OR s.CourseId = @CourseId)
  AND (@Q = '' OR s.SectionCode LIKE @LikeQ OR s.SectionName LIKE @LikeQ OR c.CourseCode LIKE @LikeQ OR c.CourseName LIKE @LikeQ)
ORDER BY s.SectionName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CourseId", (object)courseId ?? DBNull.Value),
                    new MySqlParameter("@Q", query),
                    new MySqlParameter("@LikeQ", "%" + query + "%")
                });
        }

        public DataTable GetByCourseAndTerm(int courseId, int academicYearId, int semesterId, int? yearLevelId)
        {
            const string sql = @"
SELECT
    s.SectionId,
    s.SectionCode,
    s.SectionName,
    s.CourseId,
    s.YearLevelId,
    s.AcademicYearId,
    s.SemesterId,
    s.Capacity
FROM section s
WHERE s.IsActive = 1
  AND s.CourseId = @CourseId
  AND s.AcademicYearId = @AcademicYearId
  AND s.SemesterId = @SemesterId
  AND (@YearLevelId IS NULL OR s.YearLevelId = @YearLevelId)
ORDER BY s.SectionName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CourseId", courseId),
                    new MySqlParameter("@AcademicYearId", academicYearId),
                    new MySqlParameter("@SemesterId", semesterId),
                    new MySqlParameter("@YearLevelId", (object)yearLevelId ?? DBNull.Value)
                });
        }

        public int Insert(Section section)
        {
            Guard.NotNull(section, nameof(section));

            const string sql = @"
INSERT INTO section
(
    SectionName,
    SectionCode,
    CourseId,
    YearLevelId,
    AcademicYearId,
    SemesterId,
    Capacity,
    IsActive,
    CreatedAt
)
VALUES
(
    @SectionName,
    @SectionCode,
    @CourseId,
    @YearLevelId,
    @AcademicYearId,
    @SemesterId,
    @Capacity,
    1,
    UTC_TIMESTAMP()
);";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SectionName", (object)section.SectionName ?? DBNull.Value),
                    new MySqlParameter("@SectionCode", (object)section.SectionCode ?? DBNull.Value),
                    new MySqlParameter("@CourseId", section.CourseId),
                    new MySqlParameter("@YearLevelId", section.YearLevelId),
                    new MySqlParameter("@AcademicYearId", section.AcademicYearId),
                    new MySqlParameter("@SemesterId", section.SemesterId),
                    new MySqlParameter("@Capacity", (object)section.Capacity ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Section section)
        {
            Guard.NotNull(section, nameof(section));

            const string sql = @"
UPDATE section
SET
    SectionName = @SectionName,
    SectionCode = @SectionCode,
    CourseId = @CourseId,
    YearLevelId = @YearLevelId,
    AcademicYearId = @AcademicYearId,
    SemesterId = @SemesterId,
    Capacity = @Capacity,
    UpdatedAt = UTC_TIMESTAMP()
WHERE SectionId = @SectionId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SectionId", section.SectionId),
                    new MySqlParameter("@SectionName", (object)section.SectionName ?? DBNull.Value),
                    new MySqlParameter("@SectionCode", (object)section.SectionCode ?? DBNull.Value),
                    new MySqlParameter("@CourseId", section.CourseId),
                    new MySqlParameter("@YearLevelId", section.YearLevelId),
                    new MySqlParameter("@AcademicYearId", section.AcademicYearId),
                    new MySqlParameter("@SemesterId", section.SemesterId),
                    new MySqlParameter("@Capacity", (object)section.Capacity ?? DBNull.Value)
                });
        }

        public void Delete(int sectionId)
        {
            const string sql = @"UPDATE section SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE SectionId = @SectionId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@SectionId", sectionId) });
        }
    }
}

