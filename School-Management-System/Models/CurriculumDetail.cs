namespace School_Management_System.Models
{
    public sealed class CurriculumDetail
    {
        private int _curriculumDetailId;
        private int _curriculumId;
        private int _subjectId;

        public int CurriculumDetailId { get { return _curriculumDetailId; } set { _curriculumDetailId = value; } }
        public int CurriculumId { get { return _curriculumId; } set { _curriculumId = value; } }
        public int SubjectId { get { return _subjectId; } set { _subjectId = value; } }
    }
}

