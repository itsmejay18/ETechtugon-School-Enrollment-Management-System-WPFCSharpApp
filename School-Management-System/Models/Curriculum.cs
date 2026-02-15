using System;

namespace School_Management_System.Models
{
    public sealed class Curriculum
    {
        private int _curriculumId;
        private int _courseId;
        private int _yearLevelId;
        private int _semesterId;
        private int _academicYearId;
        private string _name;
        private bool _isActive;
        private DateTime _createdAt;

        public int CurriculumId { get { return _curriculumId; } set { _curriculumId = value; } }
        public int CourseId { get { return _courseId; } set { _courseId = value; } }
        public int YearLevelId { get { return _yearLevelId; } set { _yearLevelId = value; } }
        public int SemesterId { get { return _semesterId; } set { _semesterId = value; } }
        public int AcademicYearId { get { return _academicYearId; } set { _academicYearId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
    }
}

