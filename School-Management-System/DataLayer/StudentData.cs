using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class StudentData : IStudentData
    {
        private readonly DatabaseHelper _db;

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
FROM Student
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
FROM Student
WHERE IsActive = 1
  AND (LastName LIKE @Q OR StudentNumber LIKE @Q)
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Q", "%" + query + "%")
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
FROM Student
WHERE StudentNumber LIKE CONCAT('STU-', YEAR(UTC_DATE()), '-', '____');";

            var result = _db.ExecuteScalar(sql, CommandType.Text, null);

            return Convert.ToString(result);
        }

        public int Insert(Student student)
        {
            Guard.NotNull(student, nameof(student));

            const string sql = @"
INSERT INTO Student
(
    StudentNumber,
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

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Student student)
        {
            Guard.NotNull(student, nameof(student));

            const string sql = @"
UPDATE Student
SET
    StudentNumber = @StudentNumber,
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

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@StudentId", student.StudentId),
                    new MySqlParameter("@StudentNumber", (object)student.StudentNumber ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)student.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)student.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)student.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Gender", (object)student.Gender ?? DBNull.Value),
                    new MySqlParameter("@BirthDate", (object)student.BirthDate ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)student.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)student.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)student.PhotoPath ?? DBNull.Value)
                });
        }

        public void Delete(int studentId)
        {
            const string sql = @"UPDATE Student SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE StudentId = @StudentId;";
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
FROM Enrollment e
INNER JOIN EnrollmentDetails ed ON ed.EnrollmentId = e.EnrollmentId
INNER JOIN Subject s ON s.SubjectId = ed.SubjectId
LEFT JOIN Section sec ON sec.SectionId = e.SectionId
LEFT JOIN YearLevel yl ON yl.YearLevelId = e.YearLevelId
LEFT JOIN Semester sem ON sem.SemesterId = e.SemesterId
LEFT JOIN AcademicYear ay ON ay.AcademicYearId = e.AcademicYearId
LEFT JOIN ClassSchedule cs ON cs.ClassScheduleId = ed.ClassScheduleId
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

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM Student WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
