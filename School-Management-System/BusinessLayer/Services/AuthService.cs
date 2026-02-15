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

        public AuthService(IUserData userData)
        {
            _userData = userData ?? throw new ArgumentNullException(nameof(userData));
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

            var existing = _userData.GetByUsername(username.Trim());
            if (existing == null || !existing.IsActive)
            {
                errorMessage = Messages.InvalidCredentials;
                return false;
            }

            if (!PasswordHasher.Verify(password, existing.PasswordSalt, existing.PasswordHash))
            {
                errorMessage = Messages.InvalidCredentials;
                return false;
            }

            _userData.UpdateLastLogin(existing.UserId);
            user = existing;
            return true;
        }
    }
}

