using System;
using System.Data;
using School_Management_System.BusinessLayer.Security;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class UserManagementService
    {
        private readonly IUserManagementData _userData;

        public UserManagementService(IUserManagementData userData)
        {
            _userData = userData ?? throw new ArgumentNullException(nameof(userData));
        }

        public DataTable GetUsers(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _userData.GetAll();
            }

            return _userData.Search(searchText);
        }

        public ValidationResult ValidateNewUser(string username, string password, string role)
        {
            var vr = new ValidationResult();
            if (string.IsNullOrWhiteSpace(username)) vr.Add("Username is required.");
            if (string.IsNullOrWhiteSpace(password)) vr.Add("Password is required.");
            if (string.IsNullOrWhiteSpace(role)) vr.Add("Role is required.");
            if (!string.IsNullOrWhiteSpace(username) && _userData.UsernameExists(username.Trim(), null)) vr.Add("Username already exists.");
            return vr;
        }

        public ValidationResult ValidateUpdateUser(int userId, string username, string role)
        {
            var vr = new ValidationResult();
            if (userId <= 0) vr.Add("User is required.");
            if (string.IsNullOrWhiteSpace(username)) vr.Add("Username is required.");
            if (string.IsNullOrWhiteSpace(role)) vr.Add("Role is required.");
            if (!string.IsNullOrWhiteSpace(username) && _userData.UsernameExists(username.Trim(), userId)) vr.Add("Username already exists.");
            return vr;
        }

        public int Create(string username, string password, string role, string displayName, bool isActive)
        {
            var res = PasswordHasher.HashPassword(password);

            var user = new User
            {
                Username = username.Trim(),
                PasswordHash = res.Hash,
                PasswordSalt = res.Salt,
                Role = role,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            return _userData.Insert(user);
        }

        public void Update(int userId, string username, string role, string displayName, bool isActive)
        {
            var user = new User
            {
                UserId = userId,
                Username = username.Trim(),
                Role = role,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
                IsActive = isActive
            };

            _userData.Update(user);
        }

        public void ResetPassword(int userId, string newPassword)
        {
            var res = PasswordHasher.HashPassword(newPassword);
            _userData.SetPassword(userId, res.Hash, res.Salt);
        }

        public void SetActive(int userId, bool isActive)
        {
            _userData.SetActive(userId, isActive);
        }
    }
}

