using System;
using School_Management_System.BusinessLayer.Security;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class AuthService
    {
        private readonly IUserData _userData;
        private readonly ActivityLogService _activityLogService;

        public AuthService(IUserData userData, ActivityLogService activityLogService = null)
        {
            _userData = userData ?? throw new ArgumentNullException(nameof(userData));
            _activityLogService = activityLogService;
        }

        /// <summary>
        /// Validates user credentials and returns the authenticated user on success.
        /// </summary>
        public bool TryLogin(string username, string password, out User user, out string errorMessage)
        {
            user = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessage = Messages.RequiredFieldsMissing;
                return false;
            }

            var safeUsername = username.Trim();
            var existing = _userData.GetByUsername(safeUsername);
            if (existing == null || !existing.IsActive)
            {
                errorMessage = Messages.InvalidCredentials;
                _activityLogService?.LogLoginFailure(safeUsername);
                return false;
            }

            if (!PasswordHasher.Verify(password, existing.PasswordSalt, existing.PasswordHash))
            {
                errorMessage = Messages.InvalidCredentials;
                _activityLogService?.LogLoginFailure(safeUsername);
                return false;
            }

            _userData.UpdateLastLogin(existing.UserId);
            _activityLogService?.LogLoginSuccess(existing);
            user = existing;
            return true;
        }
    }
}

