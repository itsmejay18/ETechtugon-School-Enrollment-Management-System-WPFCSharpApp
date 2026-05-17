using System;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class StudentData : IStudentData
    {
        private readonly DatabaseHelper _db;
    private bool? _hasPhotoDataColumn;

    public StudentData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    StudentId,
    StudentNumber,
    StudentType,
    CurriculumId,
    AcademicStatus,
    FirstName,
    LastName,
    MiddleName,
    Gender,
    BirthDate,
    Email,
    Phone,
    Address,
    PhotoPath,
    CreatedAt
FROM student
WHERE IsActive = 1
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    StudentId,
    StudentNumber,
    StudentType,
    CurriculumId,
    AcademicStatus,
    FirstName,
    LastName,
    MiddleName,
    Gender,
    BirthDate,
    Email,
    Phone,
    Address,
    PhotoPath,
    CreatedAt
FROM student
WHERE IsActive = 1
  AND (LastName LIKE @Q OR FirstName LIKE @Q OR StudentNumber LIKE @Q OR StudentType LIKE @Q)
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Q", "%" + query + "%")
                });
        }

        public DataTable Search(string query, string studentType)
        {
            query = (query ?? string.Empty).Trim();
            studentType = (studentType ?? string.Empty).Trim();

            const string sql = @"
SELECT
    StudentId,
    StudentNumber,
    StudentType,
    CurriculumId,
    AcademicStatus,
    FirstName,
    LastName,
    MiddleName,
    Gender,
    BirthDate,
    Email,
    Phone,
    Address,
    PhotoPath,
    CreatedAt
FROM student
WHERE IsActive = 1
  AND (@StudentType = '' OR StudentType = @StudentType)
  AND (@Q = '' OR LastName LIKE @LikeQ OR FirstName LIKE @LikeQ OR StudentNumber LIKE @LikeQ)
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentType", studentType),
                    new MySqlParameter("@Q", query),
                    new MySqlParameter("@LikeQ", "%" + query + "%")
                });
        }

        public string GetNextStudentNumber()
        {
            const string sql = @"
SELECT CONCAT(
    'STU-',
    YEAR(UTC_DATE()),
    '-',
    LPAD(IFNULL(MAX(CAST(RIGHT(StudentNumber, 4) AS UNSIGNED)), 0) + 1, 4, '0')
)
FROM student
WHERE StudentNumber LIKE CONCAT('STU-', YEAR(UTC_DATE()), '-', '____');";

            var result = _db.ExecuteScalar(sql, CommandType.Text, null);

            return Convert.ToString(result);
        }

        public int Insert(Student student)
        {
            Guard.NotNull(student, nameof(student));

            var includePhotoData = HasPhotoDataColumn();
            var sql = includePhotoData ? @"
INSERT INTO student
(
    StudentNumber,
    StudentType,
    CurriculumId,
    AcademicStatus,
    FirstName,
    LastName,
    MiddleName,
    Gender,
    BirthDate,
    Email,
    Phone,
    Address,
    PhotoPath,
    PhotoData,
    IsActive,
    CreatedAt
)
VALUES
(
    @StudentNumber,
    @StudentType,
    @CurriculumId,
    @AcademicStatus,
    @FirstName,
    @LastName,
    @MiddleName,
    @Gender,
    @BirthDate,
    @Email,
    @Phone,
    @Address,
    @PhotoPath,
    @PhotoData,
    1,
    UTC_TIMESTAMP()
);" : @"
INSERT INTO student
(
    StudentNumber,
    StudentType,
    CurriculumId,
    AcademicStatus,
    FirstName,
    LastName,
    MiddleName,
    Gender,
    BirthDate,
    Email,
    Phone,
    Address,
    PhotoPath,
    IsActive,
    CreatedAt
)
VALUES
(
    @StudentNumber,
    @StudentType,
    @CurriculumId,
    @AcademicStatus,
    @FirstName,
    @LastName,
    @MiddleName,
    @Gender,
    @BirthDate,
    @Email,
    @Phone,
    @Address,
    @PhotoPath,
    1,
    UTC_TIMESTAMP()
);";

            var parameters = includePhotoData
                ? new[]
                {
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@StudentType", (object)NormalizeStudentType(student.StudentType) ?? DBNull.Value),
                    new MySqlParameter("@CurriculumId", (object)student.CurriculumId ?? DBNull.Value),
                    new MySqlParameter("@AcademicStatus", (object)NormalizeAcademicStatus(student.AcademicStatus) ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@PhotoData", (object)student.PhotoData ?? DBNull.Value)
                }
                : new[]
                {
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@StudentType", (object)NormalizeStudentType(student.StudentType) ?? DBNull.Value),
                    new MySqlParameter("@CurriculumId", (object)student.CurriculumId ?? DBNull.Value),
                    new MySqlParameter("@AcademicStatus", (object)NormalizeAcademicStatus(student.AcademicStatus) ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value)
                };

            var id = _db.ExecuteInsert(sql, CommandType.Text, parameters);
            return Convert.ToInt32(id);
        }

        public void Update(Student student)
        {
            Guard.NotNull(student, nameof(student));

            var includePhotoData = HasPhotoDataColumn();
            var sql = includePhotoData ? @"
UPDATE student
SET
    StudentNumber = @StudentNumber,
    StudentType = @StudentType,
    CurriculumId = @CurriculumId,
    AcademicStatus = @AcademicStatus,
    FirstName = @FirstName,
    LastName = @LastName,
    MiddleName = @MiddleName,
    Gender = @Gender,
    BirthDate = @BirthDate,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    PhotoPath = @PhotoPath,
    PhotoData = @PhotoData,
    UpdatedAt = UTC_TIMESTAMP()
WHERE StudentId = @StudentId;" : @"
UPDATE student
SET
    StudentNumber = @StudentNumber,
    StudentType = @StudentType,
    CurriculumId = @CurriculumId,
    AcademicStatus = @AcademicStatus,
    FirstName = @FirstName,
    LastName = @LastName,
    MiddleName = @MiddleName,
    Gender = @Gender,
    BirthDate = @BirthDate,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    PhotoPath = @PhotoPath,
    UpdatedAt = UTC_TIMESTAMP()
WHERE StudentId = @StudentId;";

            var parameters = includePhotoData
                ? new[]
                {
                    new MySqlParameter("@StudentId", student.StudentId),
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@StudentType", (object)NormalizeStudentType(student.StudentType) ?? DBNull.Value),
                    new MySqlParameter("@CurriculumId", (object)student.CurriculumId ?? DBNull.Value),
                    new MySqlParameter("@AcademicStatus", (object)NormalizeAcademicStatus(student.AcademicStatus) ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@PhotoData", (object)student.PhotoData ?? DBNull.Value)
                }
                : new[]
                {
                    new MySqlParameter("@StudentId", student.StudentId),
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@StudentType", (object)NormalizeStudentType(student.StudentType) ?? DBNull.Value),
                    new MySqlParameter("@CurriculumId", (object)student.CurriculumId ?? DBNull.Value),
                    new MySqlParameter("@AcademicStatus", (object)NormalizeAcademicStatus(student.AcademicStatus) ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value)
                };

            _db.ExecuteNonQuery(sql, CommandType.Text, parameters);
        }

        public void Delete(int studentId)
        {
            const string sql = @"UPDATE student SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE StudentId = @StudentId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentId", studentId)
                });
        }

        public DataTable GetEnrolledSubjects(int studentId, int? academicYearId, int? semesterId)
        {
            const string sql = @"
SELECT
    e.EnrollmentNumber,
    e.EnrollDate,
    ay.Name AS AcademicYear,
    sem.Name AS Semester,
    yl.Name AS YearLevel,
    sec.SectionName,
    s.SubjectCode,
    s.SubjectName,
    ed.Units,
    ed.Grade,
    cs.DayOfWeek,
    cs.StartTime,
    cs.EndTime,
    cs.Room
FROM enrollment e
INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
INNER JOIN subject s ON s.SubjectId = ed.SubjectId
LEFT JOIN section sec ON sec.SectionId = e.SectionId
LEFT JOIN yearlevel yl ON yl.YearLevelId = e.YearLevelId
LEFT JOIN semester sem ON sem.SemesterId = e.SemesterId
LEFT JOIN academicyear ay ON ay.AcademicYearId = e.AcademicYearId
LEFT JOIN classschedule cs ON cs.ClassScheduleId = ed.ClassScheduleId
WHERE e.StudentId = @StudentId
  AND e.Status <> 'Cancelled'
  AND (@AcademicYearId IS NULL OR e.AcademicYearId = @AcademicYearId)
  AND (@SemesterId IS NULL OR e.SemesterId = @SemesterId)
ORDER BY e.EnrollDate DESC, s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentId", studentId),
                    new MySqlParameter("@AcademicYearId", (object)academicYearId ?? DBNull.Value),
                    new MySqlParameter("@SemesterId", (object)semesterId ?? DBNull.Value)
                });
        }

        public DataTable GetEnrollmentHistory(int studentId)
        {
            const string sql = @"
SELECT
    e.EnrollmentId,
    e.EnrollmentNumber,
    e.EnrollDate,
    e.Status,
    e.StudentType,
    c.CourseCode,
    c.CourseName,
    ay.Name AS AcademicYear,
    sem.Name AS Semester,
    yl.Name AS YearLevel,
    sec.SectionCode,
    sec.SectionName,
    e.TotalUnits
FROM enrollment e
LEFT JOIN course c ON c.CourseId = e.CourseId
LEFT JOIN academicyear ay ON ay.AcademicYearId = e.AcademicYearId
LEFT JOIN semester sem ON sem.SemesterId = e.SemesterId
LEFT JOIN yearlevel yl ON yl.YearLevelId = e.YearLevelId
LEFT JOIN section sec ON sec.SectionId = e.SectionId
WHERE e.StudentId = @StudentId
  AND e.Status <> 'Cancelled'
ORDER BY e.EnrollDate DESC, e.EnrollmentId DESC;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@StudentId", studentId) });
        }

        public DataTable GetCompletedSubjects(int studentId)
        {
            const string sql = @"
SELECT DISTINCT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    ed.Grade,
    e.EnrollmentNumber,
    ay.Name AS AcademicYear,
    sem.Name AS Semester
FROM enrollment e
INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
INNER JOIN subject s ON s.SubjectId = ed.SubjectId
LEFT JOIN academicyear ay ON ay.AcademicYearId = e.AcademicYearId
LEFT JOIN semester sem ON sem.SemesterId = e.SemesterId
WHERE e.StudentId = @StudentId
  AND e.Status <> 'Cancelled'
  AND ed.Grade IS NOT NULL
  AND ed.Grade <= 3.00
ORDER BY s.SubjectCode, s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@StudentId", studentId) });
        }

        public DataTable GetFailedSubjects(int studentId)
        {
            const string sql = @"
SELECT DISTINCT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    ed.Grade,
    e.EnrollmentNumber,
    ay.Name AS AcademicYear,
    sem.Name AS Semester
FROM enrollment e
INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
INNER JOIN subject s ON s.SubjectId = ed.SubjectId
LEFT JOIN academicyear ay ON ay.AcademicYearId = e.AcademicYearId
LEFT JOIN semester sem ON sem.SemesterId = e.SemesterId
WHERE e.StudentId = @StudentId
  AND e.Status <> 'Cancelled'
  AND ed.Grade IS NOT NULL
  AND ed.Grade > 3.00
ORDER BY s.SubjectCode, s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@StudentId", studentId) });
        }

        public DataTable GetRemainingSubjects(int studentId, int? curriculumId)
        {
            const string sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    s.MaxStudents,
    s.CurrentEnrolledCount
FROM curriculumdetails cd
INNER JOIN subject s ON s.SubjectId = cd.SubjectId
WHERE (@CurriculumId IS NOT NULL AND cd.CurriculumId = @CurriculumId)
  AND NOT EXISTS (
      SELECT 1
      FROM enrollment e
      INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
      WHERE e.StudentId = @StudentId
        AND e.Status <> 'Cancelled'
        AND ed.SubjectId = s.SubjectId
        AND ed.Grade IS NOT NULL
        AND ed.Grade <= 3.00
  )
ORDER BY s.SubjectCode, s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentId", studentId),
                    new MySqlParameter("@CurriculumId", (object)curriculumId ?? DBNull.Value)
                });
        }

        public DataTable GetAcademicProfile(int studentId)
        {
            const string sql = @"
SELECT
    st.StudentId,
    st.StudentNumber,
    st.StudentType,
    st.AcademicStatus,
    st.CurriculumId,
    CONCAT(st.LastName, ', ', st.FirstName) AS StudentName,
    cur.Name AS CurriculumName,
    cur.CurriculumType,
    c.CourseCode,
    c.CourseName,
    ay.Name AS AcademicYear,
    sem.Name AS Semester,
    yl.Name AS YearLevel
FROM student st
LEFT JOIN curriculum cur ON cur.CurriculumId = st.CurriculumId
LEFT JOIN course c ON c.CourseId = cur.CourseId
LEFT JOIN academicyear ay ON ay.AcademicYearId = cur.AcademicYearId
LEFT JOIN semester sem ON sem.SemesterId = cur.SemesterId
LEFT JOIN yearlevel yl ON yl.YearLevelId = cur.YearLevelId
WHERE st.StudentId = @StudentId
LIMIT 1;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@StudentId", studentId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM student WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }

        public byte[] GetPhotoData(int studentId)
        {
            if (!HasPhotoDataColumn()) return null;
            const string sql = @"SELECT PhotoData FROM student WHERE StudentId = @StudentId LIMIT 1;";
            var result = _db.ExecuteScalar(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@StudentId", studentId) });
            if (result == null || result == DBNull.Value) return null;
            return (byte[])result;
        }

        private bool HasPhotoDataColumn()
        {
            if (_hasPhotoDataColumn.HasValue) return _hasPhotoDataColumn.Value;
            const string sql = @"
SELECT COUNT(1)
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = 'student'
  AND column_name = 'PhotoData';";
            _hasPhotoDataColumn = Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null)) > 0;
            return _hasPhotoDataColumn.Value;
        }

        private static string NormalizeStudentType(string value)
        {
            if (string.Equals(value, "Irregular", StringComparison.OrdinalIgnoreCase))
            {
                return "Irregular";
            }

            if (string.Equals(value, "Summer", StringComparison.OrdinalIgnoreCase))
            {
                return "Summer";
            }

            return "Regular";
        }

        private static string NormalizeAcademicStatus(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Active" : value.Trim();
        }
    }
}

