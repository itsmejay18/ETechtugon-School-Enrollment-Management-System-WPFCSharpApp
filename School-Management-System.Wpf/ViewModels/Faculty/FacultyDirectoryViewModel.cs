using System;
using System.Data;
using System.Windows.Media;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Faculty
{
    public sealed class FacultyDirectoryViewModel : DataTableWorkspaceViewModel
    {
        private readonly FacultyService _facultyService;
        private readonly ClassScheduleService _classScheduleService;
        private ImageSource _facultyPhoto;
        private DataView _assignments;
        private string _profileSummary;

        public FacultyDirectoryViewModel(FacultyService facultyService, ClassScheduleService classScheduleService)
            : base(
                "Faculty directory",
                "Review teaching staff records and class assignments from the live academic database.",
                "Search by faculty code, name, or email",
                search => facultyService.GetFaculty(search))
        {
            _facultyService = facultyService ?? throw new ArgumentNullException(nameof(facultyService));
            _classScheduleService = classScheduleService ?? throw new ArgumentNullException(nameof(classScheduleService));
            ProfileSummary = "Select a faculty record to inspect assignments and profile information.";
            Refresh();
        }

        public ImageSource FacultyPhoto
        {
            get { return _facultyPhoto; }
            private set
            {
                if (SetProperty(ref _facultyPhoto, value))
                {
                    OnPropertyChanged(nameof(HasPhoto));
                }
            }
        }

        public bool HasPhoto
        {
            get { return FacultyPhoto != null; }
        }

        public DataView Assignments
        {
            get { return _assignments; }
            private set { SetProperty(ref _assignments, value); }
        }

        public string ProfileSummary
        {
            get { return _profileSummary; }
            private set { SetProperty(ref _profileSummary, value); }
        }

        protected override void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord, "FacultyId", "PhotoPath");
            LoadSelectedFacultyData();
        }

        public override void Refresh()
        {
            base.Refresh();
            if (!HasRecords)
            {
                FacultyPhoto = null;
                Assignments = new DataView(new DataTable());
                ProfileSummary = "No faculty records matched the current search.";
            }
        }

        private void LoadSelectedFacultyData()
        {
            if (CurrentSelectedRecord == null)
            {
                FacultyPhoto = null;
                Assignments = new DataView(new DataTable());
                ProfileSummary = "Select a faculty record to inspect assignments and profile information.";
                return;
            }

            try
            {
                var facultyId = Convert.ToInt32(CurrentSelectedRecord["FacultyId"]);
                var facultyCode = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["FacultyCode"]);
                var lastName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["LastName"]);
                var firstName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["FirstName"]);

                FacultyPhoto = WpfUiDataHelper.LoadImage(_facultyService.GetPhotoData(facultyId));
                Assignments = (_classScheduleService.GetByFaculty(facultyId) ?? new DataTable()).DefaultView;
                ProfileSummary = facultyCode + " | " + lastName + ", " + firstName;
            }
            catch
            {
                FacultyPhoto = null;
                Assignments = new DataView(new DataTable());
                ProfileSummary = "Faculty details loaded, but schedule assignments are unavailable right now.";
            }
        }
    }
}
