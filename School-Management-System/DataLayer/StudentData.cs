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
    CreatedAt
FROM Student
WHERE IsActive = 1
  AND (
        StudentNumber LIKE @Q
     OR FirstName LIKE @Q
     OR LastName LIKE @Q
     OR MiddleName LIKE @Q
     OR Email LIKE @Q
  )
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
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value)
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
                    new MySqlParameter("@Address", (object)student.Address ?? DBNull.Value)
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

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM Student WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
