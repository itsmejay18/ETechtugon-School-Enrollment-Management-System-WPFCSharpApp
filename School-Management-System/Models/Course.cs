using System;

namespace School_Management_System.Models
{
    public sealed class Course
    {
        private int _courseId;
        private string _courseCode;
        private string _courseName;
        private string _description;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int CourseId { get { return _courseId; } set { _courseId = value; } }
        public string CourseCode { get { return _courseCode; } set { _courseCode = value; } }
        public string CourseName { get { return _courseName; } set { _courseName = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}

