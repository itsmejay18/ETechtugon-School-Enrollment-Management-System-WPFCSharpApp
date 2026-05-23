using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Presentation.Helpers;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels.Students
{
    public sealed class StudentDirectoryViewModel : DataTableWorkspaceViewModel
    {
        private readonly StudentService _studentService;
        private readonly SystemSettingService _systemSettingService;
        private readonly ActivityLogService _activityLogService;
        private readonly CurriculumService _curriculumService;
        private ImageSource _studentPhoto;
        private DataView _enrolledSubjects;
        private DataView _enrollmentHistory;
        private DataView _completedSubjects;
        private DataView _failedSubjects;
        private DataView _remainingSubjects;
        private string _profileSummary;
        private int _editingStudentId;
        private bool _isEditorActive;
        private string _selectedStudentTypeFilter;
        private string _studentNumber;
        private string _selectedStudentType;
        private string _academicStatus;
        private LookupOptionViewModel _selectedCurriculum;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _selectedGender;
        private DateTime? _birthDate;
        private string _email;
        private string _phone;
        private string _address;
        private string _modalStatusMessage;
        private byte[] _currentPhotoBytes;
        private byte[] _selectedPhotoBytes;
        private string _currentPhotoPath;
        private string _selectedPhotoSourcePath;

        public StudentDirectoryViewModel(StudentService studentService, SystemSettingService systemSettingService, ActivityLogService activityLogService, CurriculumService curriculumService = null)
            : base(
                "Student Profile",
                "Search, filter, and review student profile records loaded directly from the live school database.",
                "Search by student number or last name",
                search => studentService.GetStudents(search))
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _systemSettingService = systemSettingService ?? throw new ArgumentNullException(nameof(systemSettingService));
            _activityLogService = activityLogService;
            _curriculumService = curriculumService;

            StudentTypeFilterOptions = new ObservableCollection<string> { "All", "Regular", "Irregular", "Summer" };
            StudentTypeOptions = new ObservableCollection<string> { "Regular", "Irregular", "Summer" };
            GenderOptions = new ObservableCollection<string> { string.Empty, "Male", "Female", "Other" };
            CurriculumOptions = new ObservableCollection<LookupOptionViewModel>();
            _selectedStudentTypeFilter = "All";
            EnrolledSubjects = new DataView(new DataTable());
            EnrollmentHistory = new DataView(new DataTable());
            CompletedSubjects = new DataView(new DataTable());
            FailedSubjects = new DataView(new DataTable());
            RemainingSubjects = new DataView(new DataTable());
            ProfileSummary = "Select a student record to load profile details and enrolled subjects.";
            ModalStatusMessage = "Use Add beside the filter. Edit and Delete are available after you open the selected student modal.";

            AddCommand = new RelayCommand(BeginAdd);
            EditCommand = new RelayCommand(BeginEdit, () => SelectedRecord != null && !IsEditorActive);
            DeleteCommand = new RelayCommand(DeleteCurrent, () => SelectedRecord != null && !IsEditorActive);
            SaveCommand = new RelayCommand(Save, () => IsEditorActive);
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditorActive);
            CloseModalCommand = new RelayCommand(CloseModal);
            UploadPhotoCommand = new RelayCommand(UploadPhoto, () => IsEditorActive);
            RemovePhotoCommand = new RelayCommand(RemovePhoto, () => IsEditorActive && HasPhoto);

            LoadCurriculumOptions();
            Refresh();
        }

        public ObservableCollection<string> StudentTypeFilterOptions { get; private set; }
        public ObservableCollection<string> StudentTypeOptions { get; private set; }
        public ObservableCollection<string> GenderOptions { get; private set; }
        public ObservableCollection<LookupOptionViewModel> CurriculumOptions { get; private set; }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand CloseModalCommand { get; private set; }
        public RelayCommand UploadPhotoCommand { get; private set; }
        public RelayCommand RemovePhotoCommand { get; private set; }

        public ImageSource StudentPhoto
        {
            get { return _studentPhoto; }
            private set
            {
                if (SetProperty(ref _studentPhoto, value))
                {
                    OnPropertyChanged(nameof(HasPhoto));
                    RemovePhotoCommand.RaiseCanExecuteChanged();
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

        public DataView EnrollmentHistory
        {
            get { return _enrollmentHistory; }
            private set { SetProperty(ref _enrollmentHistory, value); }
        }

        public DataView CompletedSubjects
        {
            get { return _completedSubjects; }
            private set { SetProperty(ref _completedSubjects, value); }
        }

        public DataView FailedSubjects
        {
            get { return _failedSubjects; }
            private set { SetProperty(ref _failedSubjects, value); }
        }

        public DataView RemainingSubjects
        {
            get { return _remainingSubjects; }
            private set { SetProperty(ref _remainingSubjects, value); }
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

        public bool IsEditorActive
        {
            get { return _isEditorActive; }
            private set
            {
                if (SetProperty(ref _isEditorActive, value))
                {
                    OnPropertyChanged(nameof(IsEditorReadOnly));
                    OnPropertyChanged(nameof(IsPreviewMode));
                    OnPropertyChanged(nameof(HasExistingStudent));
                    OnPropertyChanged(nameof(DetailsModalTitle));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                    RaiseCommandStates();
                }
            }
        }

        public bool IsEditorReadOnly
        {
            get { return !IsEditorActive; }
        }

        public bool IsPreviewMode
        {
            get { return !IsEditorActive; }
        }

        public bool HasExistingStudent
        {
            get { return _editingStudentId > 0; }
        }

        public string SelectedStudentTypeFilter
        {
            get { return _selectedStudentTypeFilter; }
            set
            {
                if (SetProperty(ref _selectedStudentTypeFilter, value))
                {
                    Refresh();
                }
            }
        }

        public string StudentNumber
        {
            get { return _studentNumber; }
            set
            {
                if (SetProperty(ref _studentNumber, value))
                {
                    OnPropertyChanged(nameof(DetailsModalTitle));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    OnPropertyChanged(nameof(DetailsModalTitle));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    OnPropertyChanged(nameof(DetailsModalTitle));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        public string MiddleName
        {
            get { return _middleName; }
            set { SetProperty(ref _middleName, value); }
        }

        public string SelectedGender
        {
            get { return _selectedGender; }
            set { SetProperty(ref _selectedGender, value); }
        }

        public DateTime? BirthDate
        {
            get { return _birthDate; }
            set { SetProperty(ref _birthDate, value); }
        }

        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        public string Phone
        {
            get { return _phone; }
            set { SetProperty(ref _phone, value); }
        }

        public string Address
        {
            get { return _address; }
            set { SetProperty(ref _address, value); }
        }

        public string ModalStatusMessage
        {
            get { return _modalStatusMessage; }
            private set { SetProperty(ref _modalStatusMessage, value); }
        }

        protected override void OnSelectedRecordChanged()
        {
            PopulateDetailFields(CurrentSelectedRecord, "StudentId", "PhotoPath");
            if (!IsEditorActive)
            {
                LoadSelectedStudentData();
            }

            RaiseCommandStates();
        }

        public override void Refresh()
        {
            try
            {
                var selectedStudentId = CurrentSelectedRecord == null ? 0 : Convert.ToInt32(CurrentSelectedRecord["StudentId"]);
                var studentType = string.Equals(SelectedStudentTypeFilter, "All", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : SelectedStudentTypeFilter;
                var table = _studentService.GetStudents(SearchText ?? string.Empty, studentType) ?? new DataTable();
                Records = table.DefaultView;
                SelectedRecord = Records.Count > 0 ? Records[0] : null;

                if (selectedStudentId > 0)
                {
                    SelectStudentRecord(selectedStudentId);
                }

                if (Records.Count == 0)
                {
                    IsDetailsModalOpen = false;
                }

                StatusMessage = Records.Count > 0
                    ? "Loaded student profiles from the current school database."
                    : "No student profiles matched the current search and type filter.";
            }
            catch (Exception ex)
            {
                Records = new DataView(new DataTable());
                SelectedRecord = null;
                IsDetailsModalOpen = false;
                StatusMessage = "Unable to load student profiles right now.";
                MessageBox.Show(
                    ex.Message,
                    "Student Profile",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            if (!HasRecords)
            {
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                EnrollmentHistory = new DataView(new DataTable());
                CompletedSubjects = new DataView(new DataTable());
                FailedSubjects = new DataView(new DataTable());
                RemainingSubjects = new DataView(new DataTable());
                ProfileSummary = "No student profile matched the current search.";
                if (!IsEditorActive)
                {
                    ResetEditorFields(_studentService.GetNextStudentNumber());
                }
            }
        }

        protected override string GetDetailsModalTitle()
        {
            if (IsEditorActive)
            {
                return _editingStudentId == 0
                    ? "New student"
                    : (string.IsNullOrWhiteSpace(StudentNumber) ? "Edit student" : StudentNumber);
            }

            return BuildPreferredDisplayText(
                CurrentSelectedRecord,
                "Student profile",
                "StudentNumber",
                "LastName",
                "FirstName");
        }

        protected override string GetDetailsModalSubtitle()
        {
            if (IsEditorActive)
            {
                if (_editingStudentId == 0)
                {
                    return "Create a student from the outside toolbar, then complete the form in this modal.";
                }

                return BuildStudentSummary(StudentNumber, LastName, FirstName);
            }

            return string.IsNullOrWhiteSpace(ProfileSummary)
                ? "Review the selected student profile and active-term subjects."
                : ProfileSummary;
        }

        private void BeginAdd()
        {
            _editingStudentId = 0;
            ResetEditorFields(_studentService.GetNextStudentNumber());
            IsEditorActive = true;
            IsDetailsModalOpen = true;
            ProfileSummary = "Creating a new student record.";
            ModalStatusMessage = "Add starts beside the filter, then the student form opens in this modal.";
        }

        private void BeginEdit()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            LoadSelectedStudentData();
            IsEditorActive = true;
            IsDetailsModalOpen = true;
            ModalStatusMessage = "Edit and Delete belong to this modal for the selected student record.";
        }

        private void DeleteCurrent()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            var studentId = Convert.ToInt32(CurrentSelectedRecord["StudentId"]);
            var studentLabel = BuildStudentSummary(
                GetColumnValue(CurrentSelectedRecord, "StudentNumber"),
                GetColumnValue(CurrentSelectedRecord, "LastName"),
                GetColumnValue(CurrentSelectedRecord, "FirstName"));

            var result = MessageBox.Show(
                "Delete the selected student?\n\n" + studentLabel,
                "Delete Student",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _studentService.Delete(studentId);
                LogStudentTransaction(
                    AppConstants.ActivityActions.Delete,
                    studentId,
                    "Deleted student " + studentLabel + ".");
                StatusMessage = "Student deleted successfully.";
                ModalStatusMessage = "Student deleted successfully.";
                IsDetailsModalOpen = false;
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Student Directory",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            try
            {
                var student = new Student
                {
                    StudentId = _editingStudentId,
                    StudentNumber = (StudentNumber ?? string.Empty).Trim(),
                    StudentType = StudentService.NormalizeStudentType(SelectedStudentType),
                    CurriculumId = SelectedCurriculum == null || SelectedCurriculum.Id <= 0 ? (int?)null : SelectedCurriculum.Id,
                    AcademicStatus = string.IsNullOrWhiteSpace(AcademicStatus) ? "Active" : AcademicStatus.Trim(),
                    FirstName = (FirstName ?? string.Empty).Trim(),
                    LastName = (LastName ?? string.Empty).Trim(),
                    MiddleName = (MiddleName ?? string.Empty).Trim(),
                    Gender = (SelectedGender ?? string.Empty).Trim(),
                    BirthDate = BirthDate,
                    Email = (Email ?? string.Empty).Trim(),
                    Phone = (Phone ?? string.Empty).Trim(),
                    Address = (Address ?? string.Empty).Trim(),
                    PhotoPath = _currentPhotoPath,
                    PhotoData = _selectedPhotoBytes ?? _currentPhotoBytes
                };

                if (!string.IsNullOrWhiteSpace(_selectedPhotoSourcePath))
                {
                    student.PhotoPath = PhotoStorageHelper.PersistPhoto(
                        _selectedPhotoSourcePath,
                        "Students",
                        student.StudentNumber,
                        _currentPhotoPath);
                }

                var validation = _studentService.Validate(student);
                if (!validation.IsValid)
                {
                    MessageBox.Show(
                        validation.ToString(),
                        "Student Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                int selectedStudentId;
                string transactionAction;
                if (_editingStudentId == 0)
                {
                    selectedStudentId = _studentService.Create(student);
                    transactionAction = AppConstants.ActivityActions.Create;
                    StatusMessage = "Student saved successfully.";
                }
                else
                {
                    _studentService.Update(student);
                    selectedStudentId = _editingStudentId;
                    transactionAction = AppConstants.ActivityActions.Update;
                    StatusMessage = "Student updated successfully.";
                }

                LogStudentTransaction(
                    transactionAction,
                    selectedStudentId,
                    BuildStudentTransactionMessage(transactionAction, student));

                ModalStatusMessage = StatusMessage;
                _currentPhotoPath = student.PhotoPath;
                _selectedPhotoSourcePath = null;
                IsEditorActive = false;
                IsDetailsModalOpen = false;
                SearchText = string.Empty;
                Refresh();
                SelectStudentRecord(selectedStudentId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Student Directory",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            EndEdit(false);
        }

        public string SelectedStudentType
        {
            get { return _selectedStudentType; }
            set
            {
                if (SetProperty(ref _selectedStudentType, value))
                {
                    OnPropertyChanged(nameof(StudentTypeBadgeText));
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        public string StudentTypeBadgeText
        {
            get { return string.IsNullOrWhiteSpace(SelectedStudentType) ? "Regular" : SelectedStudentType; }
        }

        public string AcademicStatus
        {
            get { return _academicStatus; }
            set
            {
                if (SetProperty(ref _academicStatus, value))
                {
                    OnPropertyChanged(nameof(DetailsModalSubtitle));
                }
            }
        }

        public LookupOptionViewModel SelectedCurriculum
        {
            get { return _selectedCurriculum; }
            set { SetProperty(ref _selectedCurriculum, value); }
        }

        private void EndEdit(bool closeModal)
        {
            var wasAdding = _editingStudentId == 0;
            IsEditorActive = false;
            if (CurrentSelectedRecord != null)
            {
                LoadSelectedStudentData();
            }
            else
            {
                ResetEditorFields(_studentService.GetNextStudentNumber());
            }

            IsDetailsModalOpen = !closeModal && !wasAdding && CurrentSelectedRecord != null;
            ModalStatusMessage = wasAdding
                ? "Student add canceled."
                : "Student edit canceled. Preview mode restored.";
        }

        private void CloseModal()
        {
            if (IsEditorActive)
            {
                EndEdit(true);
                return;
            }

            IsDetailsModalOpen = false;
        }

        private void UploadPhoto()
        {
            if (!IsEditorActive)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select student photo"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            byte[] bytes;
            string photoError;
            if (!ImageFileReader.TryReadImageBytes(dialog.FileName, out bytes, out photoError))
            {
                MessageBox.Show(
                    photoError,
                    "Student Photo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _selectedPhotoSourcePath = dialog.FileName;
            _selectedPhotoBytes = bytes;
            StudentPhoto = WpfUiDataHelper.LoadImage(_selectedPhotoBytes);
            ModalStatusMessage = "Student photo loaded. Save the record to keep the new photo.";
        }

        private void RemovePhoto()
        {
            if (!IsEditorActive)
            {
                return;
            }

            _selectedPhotoBytes = null;
            _currentPhotoBytes = null;
            _selectedPhotoSourcePath = null;
            _currentPhotoPath = null;
            StudentPhoto = null;
            ModalStatusMessage = "Student photo removed. Save the record to apply the change.";
        }

        private void LoadCurriculumOptions()
        {
            CurriculumOptions.Clear();
            CurriculumOptions.Add(new LookupOptionViewModel
            {
                Id = 0,
                Title = "(No curriculum assigned)",
                Subtitle = string.Empty
            });

            if (_curriculumService == null)
            {
                SelectedCurriculum = CurriculumOptions[0];
                return;
            }

            try
            {
                var table = _curriculumService.GetCurriculums(string.Empty) ?? new DataTable();
                foreach (DataRow row in table.Rows)
                {
                    var code = table.Columns.Contains("CourseCode") ? Convert.ToString(row["CourseCode"]) : string.Empty;
                    var year = table.Columns.Contains("YearLevel") ? Convert.ToString(row["YearLevel"]) : string.Empty;
                    var semester = table.Columns.Contains("Semester") ? Convert.ToString(row["Semester"]) : string.Empty;
                    var type = table.Columns.Contains("CurriculumType") ? Convert.ToString(row["CurriculumType"]) : string.Empty;

                    CurriculumOptions.Add(new LookupOptionViewModel
                    {
                        Id = Convert.ToInt32(row["CurriculumId"]),
                        Title = Convert.ToString(row["Name"]),
                        Subtitle = string.Join(" | ", new[] { type, code, year, semester }.Where(v => !string.IsNullOrWhiteSpace(v)))
                    });
                }
            }
            catch
            {
            }

            SelectedCurriculum = CurriculumOptions.Count > 0 ? CurriculumOptions[0] : null;
        }

        private void LoadSelectedStudentData()
        {
            if (CurrentSelectedRecord == null)
            {
                _editingStudentId = 0;
                _currentPhotoPath = null;
                _selectedPhotoSourcePath = null;
                _currentPhotoBytes = null;
                _selectedPhotoBytes = null;
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                EnrollmentHistory = new DataView(new DataTable());
                CompletedSubjects = new DataView(new DataTable());
                FailedSubjects = new DataView(new DataTable());
                RemainingSubjects = new DataView(new DataTable());
                ProfileSummary = "Select a student record to load profile details and enrolled subjects.";
                if (!IsEditorActive)
                {
                    ResetEditorFields(_studentService.GetNextStudentNumber());
                }

                return;
            }

            try
            {
                _editingStudentId = Convert.ToInt32(CurrentSelectedRecord["StudentId"]);
                _currentPhotoPath = GetColumnValue(CurrentSelectedRecord, "PhotoPath");
                _selectedPhotoBytes = null;
                _selectedPhotoSourcePath = null;

                var studentNumber = GetColumnValue(CurrentSelectedRecord, "StudentNumber");
                var lastName = GetColumnValue(CurrentSelectedRecord, "LastName");
                var firstName = GetColumnValue(CurrentSelectedRecord, "FirstName");

                LoadEditorFieldsFromSelectedRecord();
                ProfileSummary = BuildProfileSummary(studentNumber, lastName, firstName, SelectedStudentType, AcademicStatus);

                try
                {
                    _currentPhotoBytes = _studentService.GetPhotoData(_editingStudentId);
                }
                catch
                {
                    _currentPhotoBytes = null;
                }

                StudentPhoto = WpfUiDataHelper.LoadImage(_currentPhotoBytes, _currentPhotoPath);

                try
                {
                    var activeTerm = _systemSettingService.GetActiveTerm();
                    var subjectTable = _studentService.GetProfileSubjects(_editingStudentId, activeTerm.AcademicYearId, activeTerm.SemesterId) ?? new DataTable();
                    EnrolledSubjects = subjectTable.DefaultView;
                    EnrollmentHistory = (_studentService.GetEnrollmentHistory(_editingStudentId) ?? new DataTable()).DefaultView;
                    CompletedSubjects = (_studentService.GetCompletedSubjects(_editingStudentId) ?? new DataTable()).DefaultView;
                    FailedSubjects = (_studentService.GetFailedSubjects(_editingStudentId) ?? new DataTable()).DefaultView;
                    var curriculumId = SelectedCurriculum == null || SelectedCurriculum.Id <= 0 ? (int?)null : SelectedCurriculum.Id;
                    RemainingSubjects = (_studentService.GetRemainingSubjects(_editingStudentId, curriculumId) ?? new DataTable()).DefaultView;
                }
                catch
                {
                    EnrolledSubjects = new DataView(new DataTable());
                    EnrollmentHistory = new DataView(new DataTable());
                    CompletedSubjects = new DataView(new DataTable());
                    FailedSubjects = new DataView(new DataTable());
                    RemainingSubjects = new DataView(new DataTable());
                    ModalStatusMessage = "Student profile loaded, but enrolled subjects are unavailable right now.";
                }
            }
            catch
            {
                _currentPhotoPath = null;
                _selectedPhotoSourcePath = null;
                _currentPhotoBytes = null;
                _selectedPhotoBytes = null;
                StudentPhoto = null;
                EnrolledSubjects = new DataView(new DataTable());
                EnrollmentHistory = new DataView(new DataTable());
                CompletedSubjects = new DataView(new DataTable());
                FailedSubjects = new DataView(new DataTable());
                RemainingSubjects = new DataView(new DataTable());
                ProfileSummary = "Student details loaded, but related subject records are unavailable right now.";
            }
        }

        private void LoadEditorFieldsFromSelectedRecord()
        {
            if (CurrentSelectedRecord == null)
            {
                return;
            }

            StudentNumber = GetColumnValue(CurrentSelectedRecord, "StudentNumber");
            SelectedStudentType = StudentService.NormalizeStudentType(GetColumnValue(CurrentSelectedRecord, "StudentType"));
            AcademicStatus = string.IsNullOrWhiteSpace(GetColumnValue(CurrentSelectedRecord, "AcademicStatus"))
                ? "Active"
                : GetColumnValue(CurrentSelectedRecord, "AcademicStatus");
            SelectedCurriculum = FindCurriculumOption(GetNullableInt(CurrentSelectedRecord, "CurriculumId"));
            FirstName = GetColumnValue(CurrentSelectedRecord, "FirstName");
            LastName = GetColumnValue(CurrentSelectedRecord, "LastName");
            MiddleName = GetColumnValue(CurrentSelectedRecord, "MiddleName");
            SelectedGender = GetColumnValue(CurrentSelectedRecord, "Gender");
            BirthDate = CurrentSelectedRecord.Row.Table.Columns.Contains("BirthDate") && CurrentSelectedRecord["BirthDate"] != DBNull.Value
                ? (DateTime?)Convert.ToDateTime(CurrentSelectedRecord["BirthDate"])
                : null;
            Email = GetColumnValue(CurrentSelectedRecord, "Email");
            Phone = GetColumnValue(CurrentSelectedRecord, "Phone");
            Address = GetColumnValue(CurrentSelectedRecord, "Address");
            ModalStatusMessage = "Use Add beside the filter. Edit and Delete are available in this modal.";
        }

        private void ResetEditorFields(string studentNumber)
        {
            StudentNumber = studentNumber ?? string.Empty;
            SelectedStudentType = "Regular";
            AcademicStatus = "Active";
            SelectedCurriculum = CurriculumOptions.Count > 0 ? CurriculumOptions[0] : null;
            FirstName = string.Empty;
            LastName = string.Empty;
            MiddleName = string.Empty;
            SelectedGender = string.Empty;
            BirthDate = null;
            Email = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            _currentPhotoPath = null;
            _selectedPhotoSourcePath = null;
            _currentPhotoBytes = null;
            _selectedPhotoBytes = null;
            StudentPhoto = null;
            EnrolledSubjects = new DataView(new DataTable());
            EnrollmentHistory = new DataView(new DataTable());
            CompletedSubjects = new DataView(new DataTable());
            FailedSubjects = new DataView(new DataTable());
            RemainingSubjects = new DataView(new DataTable());
        }

        private void SelectStudentRecord(int studentId)
        {
            if (Records == null)
            {
                return;
            }

            foreach (DataRowView row in Records)
            {
                if (row != null && Convert.ToInt32(row["StudentId"]) == studentId)
                {
                    SelectedRecord = row;
                    return;
                }
            }

            SelectedRecord = Records.Count > 0 ? Records[0] : null;
        }

        private LookupOptionViewModel FindCurriculumOption(int? curriculumId)
        {
            if (!curriculumId.HasValue || CurriculumOptions == null)
            {
                return CurriculumOptions == null || CurriculumOptions.Count == 0 ? null : CurriculumOptions[0];
            }

            foreach (var option in CurriculumOptions)
            {
                if (option != null && option.Id == curriculumId.Value)
                {
                    return option;
                }
            }

            return CurriculumOptions.Count > 0 ? CurriculumOptions[0] : null;
        }

        private void RaiseCommandStates()
        {
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            OpenDetailsCommand.RaiseCanExecuteChanged();
            UploadPhotoCommand.RaiseCanExecuteChanged();
            RemovePhotoCommand.RaiseCanExecuteChanged();
        }

        private static string BuildStudentSummary(string studentNumber, string lastName, string firstName)
        {
            var studentLabel = string.IsNullOrWhiteSpace(studentNumber) ? "Student" : studentNumber;
            var fullName = ((lastName ?? string.Empty) + ", " + (firstName ?? string.Empty)).Trim(' ', ',');
            return string.IsNullOrWhiteSpace(fullName)
                ? studentLabel
                : studentLabel + " | " + fullName;
        }

        private static string BuildProfileSummary(string studentNumber, string lastName, string firstName, string studentType, string academicStatus)
        {
            return BuildStudentSummary(studentNumber, lastName, firstName) +
                   " | " + StudentService.NormalizeStudentType(studentType) +
                   " | " + (string.IsNullOrWhiteSpace(academicStatus) ? "Active" : academicStatus.Trim());
        }

        private static int? GetNullableInt(DataRowView row, string columnName)
        {
            if (row == null || string.IsNullOrWhiteSpace(columnName) || !row.Row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(row[columnName]);
        }

        private string BuildStudentTransactionMessage(string action, Student student)
        {
            var label = BuildStudentSummary(
                student == null ? null : student.StudentNumber,
                student == null ? null : student.LastName,
                student == null ? null : student.FirstName);

            if (string.Equals(action, AppConstants.ActivityActions.Create, StringComparison.OrdinalIgnoreCase))
            {
                return "Created student " + label + ".";
            }

            return "Updated student " + label + ".";
        }

        private void LogStudentTransaction(string action, int? studentId, string details)
        {
            if (_activityLogService == null)
            {
                return;
            }

            _activityLogService.LogTransaction(action, AppConstants.Entities.Student, studentId, details);
        }
    }
}
