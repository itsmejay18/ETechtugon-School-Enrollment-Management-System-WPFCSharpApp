using System;
using System.Data;
using System.Data.SqlClient;
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
FROM dbo.Student
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
FROM dbo.Student
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
                    new SqlParameter("@Q", SqlDbType.NVarChar, 100) { Value = "%" + query + "%" }
                });
        }

        public string GetNextStudentNumber()
        {
            const string sql = @"
DECLARE @prefix nvarchar(20) = 'STU-' + CONVERT(nvarchar(4), YEAR(GETDATE())) + '-';
DECLARE @maxNum int =
(
    SELECT MAX(TRY_CONVERT(int, RIGHT(StudentNumber, 4)))
    FROM dbo.Student
    WHERE StudentNumber LIKE @prefix + '____'
);
SELECT @prefix + RIGHT('0000' + CONVERT(nvarchar(10), ISNULL(@maxNum, 0) + 1), 4);";

            var result = _db.ExecuteScalar(sql, CommandType.Text, null);

            return Convert.ToString(result);
        }

        public int Insert(Student student)
        {
            Guard.NotNull(student, nameof(student));

            const string sql = @"
INSERT INTO dbo.Student
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
    SYSUTCDATETIME()
);
SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = _db.ExecuteScalar(
                sql,
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@StudentNumber", SqlDbType.NVarChar, 30) { Value = (object)student.StudentNumber ?? DBNull.Value },
                    new SqlParameter("@FirstName", SqlDbType.NVarChar, 50) { Value = (object)student.FirstName ?? DBNull.Value },
                    new SqlParameter("@LastName", SqlDbType.NVarChar, 50) { Value = (object)student.LastName ?? DBNull.Value },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar, 50) { Value = (object)student.MiddleName ?? DBNull.Value },
                    new SqlParameter("@Gender", SqlDbType.NVarChar, 20) { Value = (object)student.Gender ?? DBNull.Value },
                    new SqlParameter("@BirthDate", SqlDbType.Date) { Value = (object)student.BirthDate ?? DBNull.Value },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object)student.Email ?? DBNull.Value },
                    new SqlParameter("@Phone", SqlDbType.NVarChar, 30) { Value = (object)student.Phone ?? DBNull.Value },
                    new SqlParameter("@Address", SqlDbType.NVarChar, 250) { Value = (object)student.Address ?? DBNull.Value }
                });

            return Convert.ToInt32(id);
        }

        public void Update(Student student)
        {
            Guard.NotNull(student, nameof(student));

            const string sql = @"
UPDATE dbo.Student
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
    UpdatedAt = SYSUTCDATETIME()
WHERE StudentId = @StudentId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@StudentId", SqlDbType.Int) { Value = student.StudentId },
                    new SqlParameter("@StudentNumber", SqlDbType.NVarChar, 30) { Value = (object)student.StudentNumber ?? DBNull.Value },
                    new SqlParameter("@FirstName", SqlDbType.NVarChar, 50) { Value = (object)student.FirstName ?? DBNull.Value },
                    new SqlParameter("@LastName", SqlDbType.NVarChar, 50) { Value = (object)student.LastName ?? DBNull.Value },
                    new SqlParameter("@MiddleName", SqlDbType.NVarChar, 50) { Value = (object)student.MiddleName ?? DBNull.Value },
                    new SqlParameter("@Gender", SqlDbType.NVarChar, 20) { Value = (object)student.Gender ?? DBNull.Value },
                    new SqlParameter("@BirthDate", SqlDbType.Date) { Value = (object)student.BirthDate ?? DBNull.Value },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = (object)student.Email ?? DBNull.Value },
                    new SqlParameter("@Phone", SqlDbType.NVarChar, 30) { Value = (object)student.Phone ?? DBNull.Value },
                    new SqlParameter("@Address", SqlDbType.NVarChar, 250) { Value = (object)student.Address ?? DBNull.Value }
                });
        }

        public void Delete(int studentId)
        {
            const string sql = @"UPDATE dbo.Student SET IsActive = 0, UpdatedAt = SYSUTCDATETIME() WHERE StudentId = @StudentId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@StudentId", SqlDbType.Int) { Value = studentId }
                });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM dbo.Student WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
