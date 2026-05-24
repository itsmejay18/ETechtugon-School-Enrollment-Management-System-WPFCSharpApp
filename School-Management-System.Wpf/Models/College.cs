using System;

namespace School_Management_System.Models
{
    public sealed class College
    {
        private int _collegeId;
        private string _collegeCode;
        private string _collegeName;
        private string _deanName;
        private string _description;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int CollegeId { get { return _collegeId; } set { _collegeId = value; } }
        public string CollegeCode { get { return _collegeCode; } set { _collegeCode = value; } }
        public string CollegeName { get { return _collegeName; } set { _collegeName = value; } }
        public string DeanName { get { return _deanName; } set { _deanName = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}
