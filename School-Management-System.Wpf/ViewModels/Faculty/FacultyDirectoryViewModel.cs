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
            private set
            {
                if (SetProperty(ref _profileSummary, value))
                {
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        protected override void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord, "FacultyId", "PhotoPath");
            LoadSelectedFacultyPreview();
        }

        protected override void OpenDetails()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            LoadSelectedFacultyData();
            base.OpenDetails();
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
                var photoPath = CurrentSelectedRecord.Row.Table.Columns.Contains("PhotoPath")
                    ? WpfUiDataHelper.FormatValue(CurrentSelectedRecord["PhotoPath"])
                    : string.Empty;

                FacultyPhoto = WpfUiDataHelper.LoadImage(_facultyService.GetPhotoData(facultyId), photoPath);
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

        private void LoadSelectedFacultyPreview()
        {
            if (CurrentSelectedRecord == null)
            {
                FacultyPhoto = null;
                Assignments = new DataView(new DataTable());
                ProfileSummary = "Select a faculty record to inspect assignments and profile information.";
                return;
            }

            var facultyCode = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["FacultyCode"]);
            var lastName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["LastName"]);
            var firstName = WpfUiDataHelper.FormatValue(CurrentSelectedRecord["FirstName"]);
            FacultyPhoto = null;
            Assignments = new DataView(new DataTable());
            ProfileSummary = facultyCode + " | " + lastName + ", " + firstName;
        }

        protected override string GetDetailsModalTitle()
        {
            return BuildPreferredDisplayText(
                CurrentSelectedRecord,
                "Faculty profile",
                "FacultyCode",
                "LastName",
                "FirstName");
        }

        protected override string GetDetailsModalSubtitle()
        {
            return string.IsNullOrWhiteSpace(ProfileSummary)
                ? "Review the selected faculty profile and teaching assignments."
                : ProfileSummary;
        }
    }
}
