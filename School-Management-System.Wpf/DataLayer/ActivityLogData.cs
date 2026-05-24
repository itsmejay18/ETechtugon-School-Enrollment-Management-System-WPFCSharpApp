using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class ActivityLogData : IActivityLogData
    {
        private readonly DatabaseHelper _db;
        private readonly object _schemaSync = new object();
        private bool _schemaEnsured;

        public ActivityLogData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public void Add(ActivityLog entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            EnsureSchema();

            const string sql = @"
INSERT INTO `activitylog`
(UserId, Action, Entity, EntityId, Details, MachineName, CreatedAt)
VALUES
(@UserId, @Action, @Entity, @EntityId, @Details, @MachineName, UTC_TIMESTAMP());";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@UserId", (object)entry.UserId ?? DBNull.Value),
                    new MySqlParameter("@Action", entry.Action ?? string.Empty),
                    new MySqlParameter("@Entity", string.IsNullOrWhiteSpace(entry.Entity) ? (object)DBNull.Value : entry.Entity),
                    new MySqlParameter("@EntityId", (object)entry.EntityId ?? DBNull.Value),
                    new MySqlParameter("@Details", string.IsNullOrWhiteSpace(entry.Details) ? (object)DBNull.Value : entry.Details),
                    new MySqlParameter("@MachineName", string.IsNullOrWhiteSpace(entry.MachineName) ? Environment.MachineName : entry.MachineName)
                });
        }

        public IList<ActivityLog> Search(DateTime? fromUtcInclusive, DateTime? toUtcExclusive, string usernameLike, string action, int maxRows)
        {
            EnsureSchema();

            if (maxRows <= 0)
            {
                maxRows = 100;
            }

            if (maxRows > 5000)
            {
                maxRows = 5000;
            }

            const string sql = @"
SELECT
    a.ActivityLogId,
    a.UserId,
    a.Action,
    a.Entity,
    a.EntityId,
    a.Details,
    a.MachineName,
    a.CreatedAt,
    u.Username,
    u.DisplayName
FROM `activitylog` a
LEFT JOIN `users` u ON u.UserId = a.UserId
WHERE (@FromUtc IS NULL OR a.CreatedAt >= @FromUtc)
  AND (@ToUtc IS NULL OR a.CreatedAt < @ToUtc)
  AND (@Action IS NULL OR @Action = '' OR a.Action = @Action)
  AND (
        @UsernameLike IS NULL
        OR @UsernameLike = ''
        OR u.Username LIKE @UsernameLike
        OR u.DisplayName LIKE @UsernameLike
      )
ORDER BY a.CreatedAt DESC, a.ActivityLogId DESC
LIMIT @MaxRows;";

            var dt = _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@FromUtc", fromUtcInclusive.HasValue ? (object)fromUtcInclusive.Value : DBNull.Value),
                    new MySqlParameter("@ToUtc", toUtcExclusive.HasValue ? (object)toUtcExclusive.Value : DBNull.Value),
                    new MySqlParameter("@Action", string.IsNullOrWhiteSpace(action) ? (object)DBNull.Value : action.Trim()),
                    new MySqlParameter("@UsernameLike", string.IsNullOrWhiteSpace(usernameLike) ? (object)DBNull.Value : "%" + usernameLike.Trim() + "%"),
                    new MySqlParameter("@MaxRows", maxRows)
                });

            var list = new List<ActivityLog>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                var log = new ActivityLog
                {
                    ActivityLogId = Convert.ToInt32(row["ActivityLogId"]),
                    UserId = row["UserId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["UserId"]),
                    Action = Convert.ToString(row["Action"]),
                    Entity = row["Entity"] == DBNull.Value ? null : Convert.ToString(row["Entity"]),
                    EntityId = row["EntityId"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["EntityId"]),
                    Details = row["Details"] == DBNull.Value ? null : Convert.ToString(row["Details"]),
                    MachineName = row["MachineName"] == DBNull.Value ? null : Convert.ToString(row["MachineName"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"]),
                    Username = row["Username"] == DBNull.Value ? null : Convert.ToString(row["Username"]),
                    DisplayName = row["DisplayName"] == DBNull.Value ? null : Convert.ToString(row["DisplayName"])
                };

                list.Add(log);
            }

            return list;
        }

        private void EnsureSchema()
        {
            if (_schemaEnsured)
            {
                return;
            }

            lock (_schemaSync)
            {
                if (_schemaEnsured)
                {
                    return;
                }

                const string existsSql = @"
SELECT COUNT(1)
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_name = 'activitylog';";

                var exists = Convert.ToInt32(_db.ExecuteScalar(existsSql, CommandType.Text, null)) > 0;
                if (!exists)
                {
                    throw new InvalidOperationException("Missing required table 'activitylog'. Run schema migrations before using user activity logs.");
                }
                else
                {
                    const string machineColumnSql = @"
SELECT COUNT(1)
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = 'activitylog'
  AND column_name = 'MachineName';";

                    var hasMachineName = Convert.ToInt32(_db.ExecuteScalar(machineColumnSql, CommandType.Text, null)) > 0;
                    if (!hasMachineName)
                    {
                        throw new InvalidOperationException("Missing required column 'activitylog.MachineName'. Run schema migrations before using user activity logs.");
                    }
                }

                _schemaEnsured = true;
            }
        }
    }
}

