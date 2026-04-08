using School_Management_System.Wpf.Infrastructure;

namespace School_Management_System.Wpf.ViewModels.Enrollment
{
    public sealed class EnrollmentSubjectOptionViewModel : ViewModelBase
    {
        private bool _isSelected;

        public int SubjectId { get; set; }
        public int Units { get; set; }
        public int? ClassScheduleId { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public string ScheduleText { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
    }
}
