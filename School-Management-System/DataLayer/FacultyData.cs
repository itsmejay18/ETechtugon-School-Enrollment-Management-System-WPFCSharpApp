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
        private bool? _hasPhotoPathColumn;

        public FacultyData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAllActive()
        {
            var photoColumn = HasPhotoPathColumn() ? "PhotoPath" : "NULL AS PhotoPath";
            var sql = @"
SELECT
    FacultyId,
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    " + photoColumn + @",
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

            var photoColumn = HasPhotoPathColumn() ? "PhotoPath" : "NULL AS PhotoPath";
            var sql = @"
SELECT
    FacultyId,
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    " + photoColumn + @",
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

            var includePhoto = HasPhotoPathColumn();
            var sql = includePhoto ? @"
INSERT INTO Faculty
(
    FacultyCode,
    FirstName,
    LastName,
    MiddleName,
    Email,
    Phone,
    Address,
    PhotoPath,
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
    @PhotoPath,
    @HireDate,
    1,
    UTC_TIMESTAMP()
)
"
            : @"
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

            var parameters = includePhoto
                ? new[]
                {
                    new MySqlParameter("@FacultyCode", (object)faculty.FacultyCode ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)faculty.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)faculty.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)faculty.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)faculty.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)faculty.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)faculty.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)faculty.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@HireDate", (object)faculty.HireDate ?? DBNull.Value)
                }
                : new[]
                {
                    new MySqlParameter("@FacultyCode", (object)faculty.FacultyCode ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)faculty.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)faculty.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)faculty.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)faculty.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)faculty.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)faculty.Address ?? DBNull.Value),
                    new MySqlParameter("@HireDate", (object)faculty.HireDate ?? DBNull.Value)
                };

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                parameters);

            return Convert.ToInt32(id);
        }

        public void Update(Faculty faculty)
        {
            Guard.NotNull(faculty, nameof(faculty));

            var includePhoto = HasPhotoPathColumn();
            var sql = includePhoto ? @"
UPDATE Faculty
SET
    FacultyCode = @FacultyCode,
    FirstName = @FirstName,
    LastName = @LastName,
    MiddleName = @MiddleName,
    Email = @Email,
    Phone = @Phone,
    Address = @Address,
    PhotoPath = @PhotoPath,
    HireDate = @HireDate,
    UpdatedAt = UTC_TIMESTAMP()
WHERE FacultyId = @FacultyId;"
            : @"
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

            var parameters = includePhoto
                ? new[]
                {
                    new MySqlParameter("@FacultyId", faculty.FacultyId),
                    new MySqlParameter("@FacultyCode", (object)faculty.FacultyCode ?? DBNull.Value),
                    new MySqlParameter("@FirstName", (object)faculty.FirstName ?? DBNull.Value),
                    new MySqlParameter("@LastName", (object)faculty.LastName ?? DBNull.Value),
                    new MySqlParameter("@MiddleName", (object)faculty.MiddleName ?? DBNull.Value),
                    new MySqlParameter("@Email", (object)faculty.Email ?? DBNull.Value),
                    new MySqlParameter("@Phone", (object)faculty.Phone ?? DBNull.Value),
                    new MySqlParameter("@Address", (object)faculty.Address ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)faculty.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@HireDate", (object)faculty.HireDate ?? DBNull.Value)
                }
                : new[]
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
                };

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                parameters);
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

        private bool HasPhotoPathColumn()
        {
            if (_hasPhotoPathColumn.HasValue)
            {
                return _hasPhotoPathColumn.Value;
            }

            const string sql = @"
SELECT COUNT(1)
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = 'faculty'
  AND column_name = 'PhotoPath';";

            _hasPhotoPathColumn = Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null)) > 0;
            if (!_hasPhotoPathColumn.Value)
            {
                try
                {
                    const string alterSql = @"ALTER TABLE `faculty` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`;";
                    _db.ExecuteNonQuery(alterSql, CommandType.Text, null);
                    _hasPhotoPathColumn = Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null)) > 0;
                }
                catch
                {
                    // Ignore migration failure; caller will continue without photo-column persistence.
                }
            }

            return _hasPhotoPathColumn.Value;
        }
    }
}
