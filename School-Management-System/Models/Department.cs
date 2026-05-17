using System;

namespace School_Management_System.Models
{
    public sealed class Department
    {
        private int _departmentId;
        private int? _collegeId;
        private string _departmentCode;
        private string _departmentName;
        private string _departmentHead;
        private string _description;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int DepartmentId { get { return _departmentId; } set { _departmentId = value; } }
        public int? CollegeId { get { return _collegeId; } set { _collegeId = value; } }
        public string DepartmentCode { get { return _departmentCode; } set { _departmentCode = value; } }
        public string DepartmentName { get { return _departmentName; } set { _departmentName = value; } }
        public string DepartmentHead { get { return _departmentHead; } set { _departmentHead = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}
