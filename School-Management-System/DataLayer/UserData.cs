using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class UserData : IUserData
    {
        private readonly DatabaseHelper _db;

        public UserData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public User GetByUsername(string username)
        {
            Guard.NotNullOrWhiteSpace(username, nameof(username));

            const string sql = @"
SELECT TOP 1
    UserId,
    Username,
    PasswordHash,
    PasswordSalt,
    Role,
    DisplayName,
    IsActive,
    CreatedAt,
    UpdatedAt,
    LastLoginAt
FROM dbo.Users
WHERE Username = @Username;";

            var dt = _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() }
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
                IsActive = Convert.ToBoolean(r["IsActive"]),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                UpdatedAt = r["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["UpdatedAt"]),
                LastLoginAt = r["LastLoginAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["LastLoginAt"])
            };
        }

        public void UpdateLastLogin(int userId)
        {
            const string sql = @"UPDATE dbo.Users SET LastLoginAt = SYSUTCDATETIME() WHERE UserId = @UserId;";
            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
                });
        }
    }
}

