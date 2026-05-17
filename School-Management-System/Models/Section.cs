using System;

namespace School_Management_System.Models
{
    public sealed class Section
    {
        private int _sectionId;
        private string _sectionCode;
        private string _sectionName;
        private int _courseId;
        private int _yearLevelId;
        private int _academicYearId;
        private int _semesterId;
        private int? _capacity;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int SectionId { get { return _sectionId; } set { _sectionId = value; } }
        public string SectionCode { get { return _sectionCode; } set { _sectionCode = value; } }
        public string SectionName { get { return _sectionName; } set { _sectionName = value; } }
        public int CourseId { get { return _courseId; } set { _courseId = value; } }
        public int YearLevelId { get { return _yearLevelId; } set { _yearLevelId = value; } }
        public int AcademicYearId { get { return _academicYearId; } set { _academicYearId = value; } }
        public int SemesterId { get { return _semesterId; } set { _semesterId = value; } }
        public int? Capacity { get { return _capacity; } set { _capacity = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}
