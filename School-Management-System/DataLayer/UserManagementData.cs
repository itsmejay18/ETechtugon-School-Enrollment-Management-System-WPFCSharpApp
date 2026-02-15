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

        public UserManagementData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAll()
        {
            const string sql = @"
SELECT
    UserId,
    Username,
    DisplayName,
    `Role` AS Role,
    IsActive,
    CreatedAt,
    LastLoginAt
FROM Users
ORDER BY Username;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable Search(string query)
        {
            query = (query ?? string.Empty).Trim();

            const string sql = @"
SELECT
    UserId,
    Username,
    DisplayName,
    `Role` AS Role,
    IsActive,
    CreatedAt,
    LastLoginAt
FROM Users
WHERE Username LIKE @Q OR DisplayName LIKE @Q OR `Role` LIKE @Q
ORDER BY Username;";

            return _db.ExecuteDataTable(sql, CommandType.Text, new[] { new MySqlParameter("@Q", "%" + query + "%") });
        }

        public bool UsernameExists(string username, int? excludeUserId)
        {
            Guard.NotNullOrWhiteSpace(username, nameof(username));

            const string sql = @"
SELECT COUNT(1)
FROM Users
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

            const string sql = @"
INSERT INTO Users
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

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@PasswordHash", (object)user.PasswordHash ?? DBNull.Value),
                    new MySqlParameter("@PasswordSalt", (object)user.PasswordSalt ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                });

            return Convert.ToInt32(id);
        }

        public void Update(User user)
        {
            Guard.NotNull(user, nameof(user));

            const string sql = @"
UPDATE Users
SET
    Username = @Username,
    `Role` = @Role,
    DisplayName = @DisplayName,
    IsActive = @IsActive,
    UpdatedAt = UTC_TIMESTAMP()
WHERE UserId = @UserId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", user.UserId),
                    new MySqlParameter("@Username", (object)user.Username ?? DBNull.Value),
                    new MySqlParameter("@Role", (object)user.Role ?? DBNull.Value),
                    new MySqlParameter("@DisplayName", (object)user.DisplayName ?? DBNull.Value),
                    new MySqlParameter("@IsActive", user.IsActive)
                });
        }

        public void SetPassword(int userId, byte[] passwordHash, byte[] passwordSalt)
        {
            const string sql = @"
UPDATE Users
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
            const string sql = @"UPDATE Users SET IsActive = @IsActive, UpdatedAt = UTC_TIMESTAMP() WHERE UserId = @UserId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", userId),
                    new MySqlParameter("@IsActive", isActive)
                });
        }
    }
}
