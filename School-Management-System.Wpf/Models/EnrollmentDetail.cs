namespace School_Management_System.Models
{
    public sealed class EnrollmentDetail
    {
        private int _enrollmentDetailId;
        private int _enrollmentId;
        private int _subjectId;
        private int _units;
        private int? _classScheduleId;
        private decimal? _grade;

        public int EnrollmentDetailId { get { return _enrollmentDetailId; } set { _enrollmentDetailId = value; } }
        public int EnrollmentId { get { return _enrollmentId; } set { _enrollmentId = value; } }
        public int SubjectId { get { return _subjectId; } set { _subjectId = value; } }
        public int Units { get { return _units; } set { _units = value; } }
        public int? ClassScheduleId { get { return _classScheduleId; } set { _classScheduleId = value; } }
        public decimal? Grade { get { return _grade; } set { _grade = value; } }
    }
}

