using System;

namespace School_Management_System.Models
{
    public sealed class Subject
    {
        private int _subjectId;
        private string _subjectCode;
        private string _subjectName;
        private int _units;
        private int? _courseId;
        private int? _maxStudents;
        private int _currentEnrolledCount;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int SubjectId { get { return _subjectId; } set { _subjectId = value; } }
        public string SubjectCode { get { return _subjectCode; } set { _subjectCode = value; } }
        public string SubjectName { get { return _subjectName; } set { _subjectName = value; } }
        public int Units { get { return _units; } set { _units = value; } }
        public int? CourseId { get { return _courseId; } set { _courseId = value; } }
        public int? MaxStudents { get { return _maxStudents; } set { _maxStudents = value; } }
        public int CurrentEnrolledCount { get { return _currentEnrolledCount; } set { _currentEnrolledCount = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}

