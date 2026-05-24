using System;

namespace School_Management_System.Models
{
    public sealed class AcademicYear
    {
        private int _academicYearId;
        private string _name;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private bool _isCurrent;
        private bool _isActive;

        public int AcademicYearId { get { return _academicYearId; } set { _academicYearId = value; } }
        public string Name { get { return _name; } set { _name = value; } }
        public DateTime? StartDate { get { return _startDate; } set { _startDate = value; } }
        public DateTime? EndDate { get { return _endDate; } set { _endDate = value; } }
        public bool IsCurrent { get { return _isCurrent; } set { _isCurrent = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
    }
}

