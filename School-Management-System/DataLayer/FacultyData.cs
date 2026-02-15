using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class FacultyData : IFacultyData
    {
        private readonly DatabaseHelper _db;

        public FacultyData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            const string sql = @"
SELECT
    FacultyId,
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    HireDate,
    CreatedAt
FROM Faculty
WHERE IsActive = 1
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    FacultyId,
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    HireDate,
    CreatedAt
FROM Faculty
WHERE IsActive = 1
  AND (
        FacultyCode LIKE @Q
     OR FirstName LIKE @Q
     OR LastName LIKE @Q
     OR Email LIKE @Q
  )
ORDER BY LastName, FirstName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public string GetNextFacultyCode()
        {
            const string sql = @"
SELECT CONCAT(
    'FAC-',
    YEAR(UTC_DATE()),
    '-',
    LPAD(IFNULL(MAX(CAST(RIGHT(FacultyCode, 4) AS UNSIGNED)), 0) + 1, 4, '0')
)
FROM Faculty
WHERE FacultyCode LIKE CONCAT('FAC-', YEAR(UTC_DATE()), '-', '____');";

            return Convert.ToString(_db.ExecuteScalar(sql, CommandType.Text, null));
        }

        public int Insert(Faculty faculty)
        {
            Guard.NotNull(faculty, nameof(faculty));

            const string sql = @"
INSERT INTO Faculty
(
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    HireDate,
    IsActive,
    CreatedAt
)
VALUES
(
    @FacultyCode,
    @FirstName,
    @LastName,
    @MiddleName,
    @Email,
    @Phone,
    @Address,
    @HireDate,
    1,
    UTC_TIMESTAMP()
);";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@FacultyCode", (object)faculty.FacultyCode ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)faculty.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)faculty.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)faculty.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)faculty.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)faculty.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)faculty.Address ?? DBNull.Value),
                    new MySqlParameter("@HireDate", (object)faculty.HireDate ?? DBNull.Value)
                });

            return Convert.ToInt32(id);
        }

        public void Update(Faculty faculty)
        {
            Guard.NotNull(faculty, nameof(faculty));

            const string sql = @"
UPDATE Faculty
SET
    FacultyCode = @FacultyCode,
    FirstName = @FirstName,
    LastName = @LastName,
    MiddleName = @MiddleName,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    HireDate = @HireDate,
    UpdatedAt = UTC_TIMESTAMP()
WHERE FacultyId = @FacultyId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@FacultyId", faculty.FacultyId),
                    new MySqlParameter("@FacultyCode", (object)faculty.FacultyCode ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)faculty.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)faculty.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)faculty.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)faculty.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)faculty.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)faculty.Address ?? DBNull.Value),
                    new MySqlParameter("@HireDate", (object)faculty.HireDate ?? DBNull.Value)
                });
        }

        public void Delete(int facultyId)
        {
            const string sql = @"UPDATE Faculty SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE FacultyId = @FacultyId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@FacultyId", facultyId) });
        }

        public int GetActiveCount()
        {
            const string sql = @"SELECT COUNT(1) FROM Faculty WHERE IsActive = 1;";
            return Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null));
        }
    }
}
