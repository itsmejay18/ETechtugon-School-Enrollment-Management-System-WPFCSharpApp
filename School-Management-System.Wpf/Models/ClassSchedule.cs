using System;

namespace School_Management_System.Models
{
    public sealed class ClassSchedule
    {
        private int _classScheduleId;
        private int _sectionId;
        private int _subjectId;
        private int? _facultyId;
        private string _dayOfWeek;
        private TimeSpan? _startTime;
        private TimeSpan? _endTime;
        private string _room;
        private string _remarks;
        private int _academicYearId;
        private int _semesterId;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int ClassScheduleId { get { return _classScheduleId; } set { _classScheduleId = value; } }
        public int SectionId { get { return _sectionId; } set { _sectionId = value; } }
        public int SubjectId { get { return _subjectId; } set { _subjectId = value; } }
        public int? FacultyId { get { return _facultyId; } set { _facultyId = value; } }
        public string DayOfWeek { get { return _dayOfWeek; } set { _dayOfWeek = value; } }
        public TimeSpan? StartTime { get { return _startTime; } set { _startTime = value; } }
        public TimeSpan? EndTime { get { return _endTime; } set { _endTime = value; } }
        public string Room { get { return _room; } set { _room = value; } }
        public string Remarks { get { return _remarks; } set { _remarks = value; } }
        public int AcademicYearId { get { return _academicYearId; } set { _academicYearId = value; } }
        public int SemesterId { get { return _semesterId; } set { _semesterId = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}
