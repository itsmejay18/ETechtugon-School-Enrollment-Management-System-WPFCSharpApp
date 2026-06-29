using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
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
        private bool _isEditingProfile;
        private string _editorFacultyCode;
        private string _editorFirstName;
        private string _editorLastName;
        private string _editorMiddleName;
        private string _editorEmail;
        private string _editorPhone;
        private string _editorAddress;
        private string _editorHireDate;

        public FacultyDirectoryViewModel(FacultyService facultyService, ClassScheduleService classScheduleService)
            : base(
                "Faculty directory",
                "Review teaching staff records and class assignments from the live academic database.",
                "Search by faculty code, name, or email",
                search => facultyService.GetFaculty(search))
        {
            _facultyService = facultyService ?? throw new ArgumentNullException(nameof(facultyService));
            _classScheduleService = classScheduleService ?? throw new ArgumentNullException(nameof(classScheduleService));
            EditProfileCommand = new RelayCommand(BeginEditProfile, () => CurrentSelectedRecord != null && !IsEditingProfile);
            SaveProfileCommand = new RelayCommand(SaveProfile, () => CurrentSelectedRecord != null && IsEditingProfile);
            CancelProfileEditCommand = new RelayCommand(CancelProfileEdit, () => IsEditingProfile);
            ProfileSummary = "Select a faculty record to inspect assignments and profile information.";
            Refresh();
        }

        public RelayCommand EditProfileCommand { get; private set; }
        public RelayCommand SaveProfileCommand { get; private set; }
        public RelayCommand CancelProfileEditCommand { get; private set; }

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

        public bool IsEditingProfile
        {
            get { return _isEditingProfile; }
            private set
            {
                if (SetProperty(ref _isEditingProfile, value))
                {
                    OnPropertyChanged(nameof(IsProfileReadOnly));
                    RaiseProfileCommandStates();
                }
            }
        }

        public bool IsProfileReadOnly { get { return !IsEditingProfile; } }
        public string EditorFacultyCode { get { return _editorFacultyCode; } set { SetProperty(ref _editorFacultyCode, value); } }
        public string EditorFirstName { get { return _editorFirstName; } set { SetProperty(ref _editorFirstName, value); } }
        public string EditorLastName { get { return _editorLastName; } set { SetProperty(ref _editorLastName, value); } }
        public string EditorMiddleName { get { return _editorMiddleName; } set { SetProperty(ref _editorMiddleName, value); } }
        public string EditorEmail { get { return _editorEmail; } set { SetProperty(ref _editorEmail, value); } }
        public string EditorPhone { get { return _editorPhone; } set { SetProperty(ref _editorPhone, value); } }
        public string EditorAddress { get { return _editorAddress; } set { SetProperty(ref _editorAddress, value); } }
        public string EditorHireDate { get { return _editorHireDate; } set { SetProperty(ref _editorHireDate, value); } }

        protected override void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord, "FacultyId", "PhotoPath");
            IsEditingProfile = false;
            LoadEditorFields();
            LoadSelectedFacultyPreview();
            RaiseProfileCommandStates();
        }

        protected override void OpenDetails()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            LoadSelectedFacultyData();
            LoadEditorFields();
            IsEditingProfile = false;
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

        private void BeginEditProfile()
        {
            LoadEditorFields();
            IsEditingProfile = true;
        }

        private void CancelProfileEdit()
        {
            LoadEditorFields();
            IsEditingProfile = false;
        }

        private void SaveProfile()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            DateTime? hireDate;
            if (!TryParseOptionalDate(EditorHireDate, out hireDate))
            {
                MessageBox.Show(
                    "Hire Date is invalid. Use a valid date like 2026-06-30.",
                    "Faculty Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var facultyId = Convert.ToInt32(CurrentSelectedRecord["FacultyId"]);
            var faculty = new School_Management_System.Models.Faculty
            {
                FacultyId = facultyId,
                FacultyCode = Clean(EditorFacultyCode),
                FirstName = Clean(EditorFirstName),
                LastName = Clean(EditorLastName),
                MiddleName = Clean(EditorMiddleName),
                Email = Clean(EditorEmail),
                Phone = Clean(EditorPhone),
                Address = Clean(EditorAddress),
                PhotoPath = GetColumnValue(CurrentSelectedRecord, "PhotoPath"),
                PhotoData = _facultyService.GetPhotoData(facultyId),
                HireDate = hireDate
            };

            var validation = _facultyService.Validate(faculty);
            if (!validation.IsValid)
            {
                MessageBox.Show(
                    validation.ToString(),
                    "Faculty Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                _facultyService.Update(faculty);
                StatusMessage = "Faculty profile updated successfully.";
                IsEditingProfile = false;
                Refresh();
                SelectFacultyRecord(facultyId);
                LoadSelectedFacultyData();
                IsDetailsModalOpen = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Faculty Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadEditorFields()
        {
            if (CurrentSelectedRecord == null)
            {
                EditorFacultyCode = string.Empty;
                EditorFirstName = string.Empty;
                EditorLastName = string.Empty;
                EditorMiddleName = string.Empty;
                EditorEmail = string.Empty;
                EditorPhone = string.Empty;
                EditorAddress = string.Empty;
                EditorHireDate = string.Empty;
                return;
            }

            EditorFacultyCode = GetColumnValue(CurrentSelectedRecord, "FacultyCode");
            EditorFirstName = GetColumnValue(CurrentSelectedRecord, "FirstName");
            EditorLastName = GetColumnValue(CurrentSelectedRecord, "LastName");
            EditorMiddleName = GetColumnValue(CurrentSelectedRecord, "MiddleName");
            EditorEmail = GetColumnValue(CurrentSelectedRecord, "Email");
            EditorPhone = GetColumnValue(CurrentSelectedRecord, "Phone");
            EditorAddress = GetColumnValue(CurrentSelectedRecord, "Address");
            EditorHireDate = GetColumnValue(CurrentSelectedRecord, "HireDate");
        }

        private void SelectFacultyRecord(int facultyId)
        {
            if (Records == null)
            {
                return;
            }

            foreach (DataRowView row in Records)
            {
                if (row != null && row.Row.Table.Columns.Contains("FacultyId") && Convert.ToInt32(row["FacultyId"]) == facultyId)
                {
                    SelectedRecord = row;
                    return;
                }
            }
        }

        private void RaiseProfileCommandStates()
        {
            if (EditProfileCommand != null) EditProfileCommand.RaiseCanExecuteChanged();
            if (SaveProfileCommand != null) SaveProfileCommand.RaiseCanExecuteChanged();
            if (CancelProfileEditCommand != null) CancelProfileEditCommand.RaiseCanExecuteChanged();
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

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool TryParseOptionalDate(string value, out DateTime? date)
        {
            date = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            DateTime parsed;
            if (DateTime.TryParse(value.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed) ||
                DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                date = parsed.Date;
                return true;
            }

            return false;
        }
    }
}
