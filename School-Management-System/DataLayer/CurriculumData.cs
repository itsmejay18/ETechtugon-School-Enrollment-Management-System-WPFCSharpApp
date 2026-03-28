using System;
using System.Data;
using MySqlConnector;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.DataLayer
{
    public sealed class CurriculumData : ICurriculumData
    {
        private readonly DatabaseHelper _db;

        public CurriculumData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public int? GetCurriculumId(int courseId, int yearLevelId, int semesterId, int academicYearId)
        {
            const string sql = @"
SELECT CurriculumId
FROM curriculum
WHERE IsActive = 1
  AND CourseId = @CourseId
  AND YearLevelId = @YearLevelId
  AND SemesterId = @SemesterId
  AND AcademicYearId = @AcademicYearId
LIMIT 1;";

            var result = _db.ExecuteScalar(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CourseId", courseId),
                    new MySqlParameter("@YearLevelId", yearLevelId),
                    new MySqlParameter("@SemesterId", semesterId),
                    new MySqlParameter("@AcademicYearId", academicYearId)
                });

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(result);
        }

        public int InsertCurriculum(string name, int courseId, int yearLevelId, int semesterId, int academicYearId)
        {
            const string sql = @"
INSERT INTO curriculum
(
    Name,
    CourseId,
    YearLevelId,
    SemesterId,
    AcademicYearId,
    IsActive,
    CreatedAt
)
VALUES
(
    @Name,
    @CourseId,
    @YearLevelId,
    @SemesterId,
    @AcademicYearId,
    1,
    UTC_TIMESTAMP()
);";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Name", (object)name ?? DBNull.Value),
                    new MySqlParameter("@CourseId", courseId),
                    new MySqlParameter("@YearLevelId", yearLevelId),
                    new MySqlParameter("@SemesterId", semesterId),
                    new MySqlParameter("@AcademicYearId", academicYearId)
                });

            return Convert.ToInt32(id);
        }

        public DataTable GetSubjectsForCourse(int courseId)
        {
            const string sql = @"
SELECT
    SubjectId,
    SubjectCode,
    SubjectName,
    Units,
    CourseId
FROM subject
WHERE IsActive = 1
  AND (CourseId = @CourseId OR CourseId IS NULL)
ORDER BY SubjectName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@CourseId", courseId) });
        }

        public DataTable GetCurriculumSubjects(int curriculumId)
        {
            const string sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units
FROM curriculumdetails cd
INNER JOIN subject s ON s.SubjectId = cd.SubjectId
WHERE cd.CurriculumId = @CurriculumId
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@CurriculumId", curriculumId) });
        }

        public void AddSubject(int curriculumId, int subjectId)
        {
            const string sql = @"
INSERT IGNORE INTO curriculumdetails (CurriculumId, SubjectId, CreatedAt)
VALUES (@CurriculumId, @SubjectId, UTC_TIMESTAMP());";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CurriculumId", curriculumId),
                    new MySqlParameter("@SubjectId", subjectId)
                });
        }

        public void RemoveSubject(int curriculumId, int subjectId)
        {
            const string sql = @"DELETE FROM curriculumdetails WHERE CurriculumId = @CurriculumId AND SubjectId = @SubjectId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@CurriculumId", curriculumId),
                    new MySqlParameter("@SubjectId", subjectId)
                });
        }
    }
}

