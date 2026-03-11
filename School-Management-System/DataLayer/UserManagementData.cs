using System;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class UserManagementData : IUserManagementData
    {
        private readonly DatabaseHelper _db;
        private bool? _hasPhotoPathColumn;

        public UserManagementData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAll()
        {
            var photoColumn = HasPhotoPathColumn() ? "PhotoPath" : "NULL AS PhotoPath";
            var sql = @"
SELECT
    UserId,
    Username,
    DisplayName,
    " + photoColumn + @",
    `Role` AS Role,
    IsActive,
    CreatedAt,
    LastLoginAt
FROM users
ORDER BY Username;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            var photoColumn = HasPhotoPathColumn() ? "PhotoPath" : "NULL AS PhotoPath";
            var sql = @"
SELECT
    UserId,
    Username,
    DisplayName,
    " + photoColumn + @",
    `Role` AS Role,
    IsActive,
    CreatedAt,
    LastLoginAt
FROM users
WHERE Username LIKE @Q OR DisplayName LIKE @Q OR `Role` LIKE @Q
ORDER BY Username;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public bool UsernameExists(string username, int? excludeUserId)
        {
            Guard.NotNullOrWhiteSpace(username, nameof(username));

            const string sql = @"
SELECT COUNT(1)
FROM users
WHERE Username = @Username
  AND (@ExcludeUserId IS NULL OR UserId <> @ExcludeUserId);";

            var count = Convert.ToInt32(_db.ExecuteScalar(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Username", username.Trim()),
                    new MySqlParameter("@ExcludeUserId", (object)excludeUserId ?? DBNull.Value)
                }));

            return count > 0;
        }

        public int Insert(User user)
        {
            Guard.NotNull(user, nameof(user));

            var includePhoto = HasPhotoPathColumn();
            var sql = includePhoto ? @"
INSERT INTO users
(
    Username,
    PasswordHash,
    PasswordSalt,
    `Role`,
    DisplayName,
    PhotoPath,
    IsActive,
    CreatedAt
)
VALUES
(
    @Username,
    @PasswordHash,
    @PasswordSalt,
    @Role,
    @DisplayName,
    @PhotoPath,
    @IsActive,
    UTC_TIMESTAMP()
);"
            : @"
INSERT INTO users
(
    Username,
    PasswordHash,
    PasswordSalt,
    `Role`,
    DisplayName,
    IsActive,
    CreatedAt
)
VALUES
(
    @Username,
    @PasswordHash,
    @PasswordSalt,
    @Role,
    @DisplayName,
    @IsActive,
    UTC_TIMESTAMP()
);";

            var parameters = includePhoto
                ? new[]
                {
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@PasswordHash", (object)user.PasswordHash ?? DBNull.Value),
                    new MySqlParameter("@PasswordSalt", (object)user.PasswordSalt ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)user.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                }
                : new[]
                {
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@PasswordHash", (object)user.PasswordHash ?? DBNull.Value),
                    new MySqlParameter("@PasswordSalt", (object)user.PasswordSalt ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                };

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                parameters);

            return Convert.ToInt32(id);
        }

        public void Update(User user)
        {
            Guard.NotNull(user, nameof(user));

            var includePhoto = HasPhotoPathColumn();
            var sql = includePhoto ? @"
UPDATE users
SET
    Username = @Username,
    `Role` = @Role,
    DisplayName = @DisplayName,
    PhotoPath = @PhotoPath,
    IsActive = @IsActive,
    UpdatedAt = UTC_TIMESTAMP()
WHERE UserId = @UserId;"
            : @"
UPDATE users
SET
    Username = @Username,
    `Role` = @Role,
    DisplayName = @DisplayName,
    IsActive = @IsActive,
    UpdatedAt = UTC_TIMESTAMP()
WHERE UserId = @UserId;";

            var parameters = includePhoto
                ? new[]
                {
                    new MySqlParameter("@UserId", user.UserId),
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@PhotoPath", (object)user.PhotoPath ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                }
                : new[]
                {
                    new MySqlParameter("@UserId", user.UserId),
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                };

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                parameters);
        }

        public void SetPassword(int userId, byte[] passwordHash, byte[] passwordSalt)
        {
            const string sql = @"
UPDATE users
SET
    PasswordHash = @PasswordHash,
    PasswordSalt = @PasswordSalt,
    UpdatedAt = UTC_TIMESTAMP()
WHERE UserId = @UserId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", userId),
                    new MySqlParameter("@PasswordHash", (object)passwordHash ?? DBNull.Value),
                    new MySqlParameter("@PasswordSalt", (object)passwordSalt ?? DBNull.Value)
                });
        }

        public void SetActive(int userId, bool isActive)
        {
            const string sql = @"UPDATE users SET IsActive = @IsActive, UpdatedAt = UTC_TIMESTAMP() WHERE UserId = @UserId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", userId),
                    new MySqlParameter("@IsActive", isActive)
                });
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
  AND table_name = 'users'
  AND column_name = 'PhotoPath';";

            _hasPhotoPathColumn = Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null)) > 0;
            return _hasPhotoPathColumn.Value;
        }
    }
}
