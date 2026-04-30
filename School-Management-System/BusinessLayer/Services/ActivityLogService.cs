using System;
using System.Collections.Generic;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.DataLayer.Logging;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class ActivityLogService
    {
        private readonly IActivityLogData _activityLogData;
        public event EventHandler<ActivityLoggedEventArgs> ActivityLogged;

        public ActivityLogService(IActivityLogData activityLogData)
        {
            _activityLogData = activityLogData ?? throw new ArgumentNullException(nameof(activityLogData));
        }

        public void Log(int? userId, string action, string entity, int? entityId, string details)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return;
            }

            try
            {
                var entry = new ActivityLog
                {
                    UserId = userId,
                    Action = action.Trim(),
                    Entity = string.IsNullOrWhiteSpace(entity) ? null : entity.Trim(),
                    EntityId = entityId,
                    Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim(),
                    MachineName = Environment.MachineName
                };

                _activityLogData.Add(entry);
                OnActivityLogged(entry);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("ActivityLogService.Log", ex);
            }
        }

        public void LogTransaction(string action, string entity, int? entityId, string details)
        {
            var currentUser = UserSession.CurrentUser;
            Log(
                currentUser == null ? (int?)null : currentUser.UserId,
                action,
                entity,
                entityId,
                details);
        }

        public IList<ActivityLog> Search(DateTime? fromUtcInclusive, DateTime? toUtcExclusive, string usernameLike, string action, int maxRows)
        {
            try
            {
                return _activityLogData.Search(fromUtcInclusive, toUtcExclusive, usernameLike, action, maxRows);
            }
            catch (Exception ex)
            {
                FileLogger.LogError("ActivityLogService.Search", ex);
                return new List<ActivityLog>();
            }
        }

        public void LogLoginSuccess(User user)
        {
            if (user == null)
            {
                return;
            }

            var name = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName;
            Log(
                user.UserId,
                AppConstants.ActivityActions.LoginSuccess,
                AppConstants.Entities.User,
                user.UserId,
                "User logged in: " + name + ".");
        }

        public void LogLoginFailure(string username)
        {
            var safeUsername = string.IsNullOrWhiteSpace(username) ? "(blank)" : username.Trim();
            Log(
                null,
                AppConstants.ActivityActions.LoginFailed,
                AppConstants.Entities.User,
                null,
                "Login failed for username '" + safeUsername + "'.");
        }

        public void LogLogout(User user)
        {
            if (user == null)
            {
                return;
            }

            var name = string.IsNullOrWhiteSpace(user.DisplayName) ? user.Username : user.DisplayName;
            Log(
                user.UserId,
                AppConstants.ActivityActions.Logout,
                AppConstants.Entities.User,
                user.UserId,
                "User logged out: " + name + ".");
        }

        private void OnActivityLogged(ActivityLog entry)
        {
            var handler = ActivityLogged;
            if (handler != null)
            {
                handler(this, new ActivityLoggedEventArgs(entry));
            }
        }

        public sealed class ActivityLoggedEventArgs : EventArgs
        {
            public ActivityLoggedEventArgs(ActivityLog entry)
            {
                Entry = entry;
            }

            public ActivityLog Entry { get; private set; }
        }
    }
}
