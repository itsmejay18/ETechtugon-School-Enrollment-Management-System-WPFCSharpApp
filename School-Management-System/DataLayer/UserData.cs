using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class UserData : IUserData
    {
        private readonly DatabaseHelper _db;
        private bool? _hasPhotoPathColumn;

        public UserData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public User GetByUsername(string username)
        {
            Guard.NotNullOrWhiteSpace(username, nameof(username));

            var photoColumn = HasPhotoPathColumn() ? "PhotoPath" : "NULL AS PhotoPath";
            var sql = @"
SELECT
    UserId,
    Username,
    PasswordHash,
    PasswordSalt,
    `Role` AS Role,
    DisplayName,
    " + photoColumn + @",
    IsActive,
    CreatedAt,
    UpdatedAt,
    LastLoginAt
FROM Users
WHERE Username = @Username
LIMIT 1;";

            var dt = _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Username", username.Trim())
                });

            if (dt.Rows.Count == 0)
            {
                return null;
            }

            var r = dt.Rows[0];
            return new User
            {
                UserId = Convert.ToInt32(r["UserId"]),
                Username = Convert.ToString(r["Username"]),
                PasswordHash = (byte[])r["PasswordHash"],
                PasswordSalt = (byte[])r["PasswordSalt"],
                Role = Convert.ToString(r["Role"]),
                DisplayName = Convert.ToString(r["DisplayName"]),
                PhotoPath = Convert.ToString(r["PhotoPath"]),
                IsActive = Convert.ToBoolean(r["IsActive"]),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                UpdatedAt = r["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["UpdatedAt"]),
                LastLoginAt = r["LastLoginAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastLoginAt"])
            };
        }

        public void UpdateLastLogin(int userId)
        {
            const string sql = @"UPDATE Users SET LastLoginAt = UTC_TIMESTAMP() WHERE UserId = @UserId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", userId)
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
            if (!_hasPhotoPathColumn.Value)
            {
                try
                {
                    const string alterSql = @"ALTER TABLE `users` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `DisplayName`;";
                    _db.ExecuteNonQuery(alterSql, CommandType.Text, null);
                    _hasPhotoPathColumn = Convert.ToInt32(_db.ExecuteScalar(sql, CommandType.Text, null)) > 0;
                }
                catch
                {
                    // Ignore migration failure; login can continue without photo-path column.
                }
            }

            return _hasPhotoPathColumn.Value;
        }
    }
}
