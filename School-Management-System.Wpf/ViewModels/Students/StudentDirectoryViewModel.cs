using System;
using System.Data;
using System.Windows.Media;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Students
{
    public sealed class StudentDirectoryViewModel : DataTableWorkspaceViewModel
    {
        private readonly StudentService _studentService;
        private readonly SystemSettingService _systemSettingService;
        private ImageSource _studentPhoto;
        private DataView _enrolledSubjects;
        private string _profileSummary;

        public StudentDirectoryViewModel(StudentService studentService, SystemSettingService systemSettingService)
            : base(
                "Student directory",
                "Search and review student records loaded directly from the live school database.",
                "Search by student number or last name",
                search => studentService.GetStudents(search))
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            ProfileSummary = "Select a student record to load profile details and enrolled subjects.";
            Refresh();
        }

        public ImageSource StudentPhoto
        {
            get { return _studentPhoto; }
            private set
            {
                if (SetProperty(ref _studentPhoto, value))
                {
                    OnPropertyChanged(nameof(HasPhoto));
                }
            }
        }

        public bool HasPhoto
        {
            get { return StudentPhoto != null; }
        }

        public DataView EnrolledSubjects
        {
            get { return _enrolledSubjects; }
            private set { SetProperty(ref _enrolledSubjects, value); }
        }

        public string ProfileSummary
        {
            get { return _profileSummary; }
            private set { SetProperty(ref _profileSummary, value); }
        }

        protected override void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord, "StudentId", "PhotoPath");
            LoadSelectedStudentData();
        }

        public override void Refresh()
        {
            base.Refresh();
            if (!HasRecords)
            {
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                ProfileSummary = "No student data matched the current search.";
            }
        }

        private void LoadSelectedStudentData()
        {
            if (CurrentSelectedRecord == null)
            {
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                ProfileSummary = "Select a student record to load profile details and enrolled subjects.";
                return;
            }

            try
            {
                var studentId = Convert.ToInt32(CurrentSelectedRecord["StudentId"]);
                var studentNumber = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["StudentNumber"]);
                var lastName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["LastName"]);
                var firstName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["FirstName"]);

                StudentPhoto = WpfUiDataHelper.LoadImage(_studentService.GetPhotoData(studentId));

                var activeTerm = _systemSettingService.GetActiveTerm();
                var subjectTable = _studentService.GetProfileSubjects(studentId, activeTerm.AcademicYearId, activeTerm.SemesterId) ?? new DataTable();
                EnrolledSubjects = subjectTable.DefaultView;
                ProfileSummary = studentNumber + " | " + lastName + ", " + firstName;
            }
            catch
            {
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                ProfileSummary = "Student details loaded, but related subject records are unavailable right now.";
            }
        }
    }
}
