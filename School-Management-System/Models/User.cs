using System;

namespace School_Management_System.Models
{
    public sealed class User
    {
        private int _userId;
        private string _username;
        private byte[] _passwordHash;
        private byte[] _passwordSalt;
        private string _role;
        private string _displayName;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;
        private DateTime? _lastLoginAt;

        public int UserId { get { return _userId; } set { _userId = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public byte[] PasswordHash { get { return _passwordHash; } set { _passwordHash = value; } }
        public byte[] PasswordSalt { get { return _passwordSalt; } set { _passwordSalt = value; } }
        public string Role { get { return _role; } set { _role = value; } }
        public string DisplayName { get { return _displayName; } set { _displayName = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
        public DateTime? LastLoginAt { get { return _lastLoginAt; } set { _lastLoginAt = value; } }
    }
}

