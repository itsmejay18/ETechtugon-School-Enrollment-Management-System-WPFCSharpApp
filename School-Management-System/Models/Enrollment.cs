using System;

namespace School_Management_System.Models
{
    public sealed class Enrollment
    {
        private int _enrollmentId;
        private string _enrollmentNumber;
        private int _studentId;
        private int _courseId;
        private int _academicYearId;
        private int _yearLevelId;
        private int _semesterId;
        private int _sectionId;
        private DateTime _enrollDate;
        private int _totalUnits;
        private string _status;
        private DateTime _createdAt;

        public int EnrollmentId { get { return _enrollmentId; } set { _enrollmentId = value; } }
        public string EnrollmentNumber { get { return _enrollmentNumber; } set { _enrollmentNumber = value; } }
        public int StudentId { get { return _studentId; } set { _studentId = value; } }
        public int CourseId { get { return _courseId; } set { _courseId = value; } }
        public int AcademicYearId { get { return _academicYearId; } set { _academicYearId = value; } }
        public int YearLevelId { get { return _yearLevelId; } set { _yearLevelId = value; } }
        public int SemesterId { get { return _semesterId; } set { _semesterId = value; } }
        public int SectionId { get { return _sectionId; } set { _sectionId = value; } }
        public DateTime EnrollDate { get { return _enrollDate; } set { _enrollDate = value; } }
        public int TotalUnits { get { return _totalUnits; } set { _totalUnits = value; } }
        public string Status { get { return _status; } set { _status = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
    }
}

