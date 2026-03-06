using System;

namespace School_Management_System.Models
{
    public sealed class ActivityLog
    {
        private int _activityLogId;
        private int? _userId;
        private string _action;
        private string _entity;
        private int? _entityId;
        private string _details;
        private string _machineName;
        private string _username;
        private string _displayName;
        private DateTime _createdAt;

        public int ActivityLogId { get { return _activityLogId; } set { _activityLogId = value; } }
        public int? UserId { get { return _userId; } set { _userId = value; } }
        public string Action { get { return _action; } set { _action = value; } }
        public string Entity { get { return _entity; } set { _entity = value; } }
        public int? EntityId { get { return _entityId; } set { _entityId = value; } }
        public string Details { get { return _details; } set { _details = value; } }
        public string MachineName { get { return _machineName; } set { _machineName = value; } }
        public string Username { get { return _username; } set { _username = value; } }
        public string DisplayName { get { return _displayName; } set { _displayName = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
    }
}

