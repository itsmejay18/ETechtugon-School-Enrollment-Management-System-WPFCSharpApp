using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;

namespace School_Management_System.Wpf.ViewModels.Courses
{
    public sealed class CourseManagementViewModel : ViewModelBase, Shared.IModalStateHost
    {
        private readonly CourseService _courseService;
        private readonly DepartmentService _departmentService;
        private readonly ActivityLogService _activityLogService;

        private string _searchText;
        private CourseListItemViewModel _selectedCourse;
        private string _courseCode;
        private string _courseName;
        private string _description;
        private DepartmentOptionViewModel _selectedDepartment;
        private bool _isEditorActive;
        private bool _isEditorModalOpen;
        private string _statusMessage;
        private int _editingCourseId;

        public CourseManagementViewModel(CourseService courseService, DepartmentService departmentService, ActivityLogService activityLogService)
        {
            _courseService = courseService ?? throw new ArgumentNullException(nameof(courseService));
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _activityLogService = activityLogService;

            Courses = new ObservableCollection<CourseListItemViewModel>();
            Departments = new ObservableCollection<DepartmentOptionViewModel>();

            RefreshCommand = new RelayCommand(RefreshCourses);
            AddCommand = new RelayCommand(BeginAdd);
            EditCommand = new RelayCommand(BeginEdit, () => SelectedCourse != null && !IsEditorActive);
            DeleteCommand = new RelayCommand(DeleteCurrent, () => SelectedCourse != null && !IsEditorActive);
            SaveCommand = new RelayCommand(Save, () => IsEditorActive);
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditorActive);

            LoadDepartments();
            RefreshCourses();
        }

        public ObservableCollection<CourseListItemViewModel> Courses { get; private set; }
        public ObservableCollection<DepartmentOptionViewModel> Departments { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RefreshCourses();
                }
            }
        }

        public CourseListItemViewModel SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                if (SetProperty(ref _selectedCourse, value))
                {
                    if (!IsEditorActive)
                    {
                        PreviewSelected();
                    }

                    OnPropertyChanged(nameof(EditorModalTitle));
                    OnPropertyChanged(nameof(EditorModalSubtitle));
                    RaiseCommandStates();
                }
            }
        }

        public string CourseCode
        {
            get { return _courseCode; }
            set { SetProperty(ref _courseCode, value); }
        }

        public string CourseName
        {
            get { return _courseName; }
            set { SetProperty(ref _courseName, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        public DepartmentOptionViewModel SelectedDepartment
        {
            get { return _selectedDepartment; }
            set { SetProperty(ref _selectedDepartment, value); }
        }

        public bool IsEditorActive
        {
            get { return _isEditorActive; }
            private set
            {
                if (SetProperty(ref _isEditorActive, value))
                {
                    OnPropertyChanged(nameof(IsEditorReadOnly));
                    OnPropertyChanged(nameof(EditorModeText));
                    OnPropertyChanged(nameof(EditorModalTitle));
                    OnPropertyChanged(nameof(EditorModalSubtitle));
                    RaiseCommandStates();
                }
            }
        }

        public bool IsEditorModalOpen
        {
            get { return _isEditorModalOpen; }
            private set
            {
                if (SetProperty(ref _isEditorModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            }
        }

        public bool IsModalOpen
        {
            get { return IsEditorModalOpen; }
        }

        public bool IsEditorReadOnly
        {
            get { return !IsEditorActive; }
        }

        public string EditorModeText
        {
            get
            {
                return IsEditorActive
                    ? "Editing is enabled. Save to commit changes or Cancel to return to preview mode."
                    : "Preview mode. Select a row to inspect details, then click Edit to modify.";
            }
        }

        public string EditorModalTitle
        {
            get
            {
                if (_editingCourseId == 0)
                {
                    return "New course";
                }

                return SelectedCourse == null || string.IsNullOrWhiteSpace(SelectedCourse.CourseName)
                    ? "Edit course"
                    : SelectedCourse.CourseName;
            }
        }

        public string EditorModalSubtitle
        {
            get
            {
                if (_editingCourseId == 0)
                {
                    return "Create a new course record inside the modal editor.";
                }

                return SelectedCourse == null || string.IsNullOrWhiteSpace(SelectedCourse.CourseCode)
                    ? "Update the selected course information."
                    : SelectedCourse.CourseCode + " is ready for editing in the modal workspace.";
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        private void LoadDepartments()
        {
            Departments.Clear();
            Departments.Add(new DepartmentOptionViewModel
            {
                DepartmentId = 0,
                DepartmentCode = string.Empty,
                DepartmentName = "(None)"
            });

            var dt = _departmentService.GetLookupDepartments();
            foreach (DataRow row in dt.Rows)
            {
                Departments.Add(new DepartmentOptionViewModel
                {
                    DepartmentId = Convert.ToInt32(row["DepartmentId"]),
                    DepartmentCode = Convert.ToString(row["DepartmentCode"]),
                    DepartmentName = Convert.ToString(row["DepartmentName"])
                });
            }

            SelectedDepartment = Departments.FirstOrDefault();
        }

        private void RefreshCourses()
        {
            try
            {
                var selectedCourseId = SelectedCourse == null ? 0 : SelectedCourse.CourseId;
                var dt = _courseService.GetCourses(SearchText ?? string.Empty);

                Courses.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    Courses.Add(new CourseListItemViewModel
                    {
                        CourseId = Convert.ToInt32(row["CourseId"]),
                        CourseCode = Convert.ToString(row["CourseCode"]),
                        CourseName = Convert.ToString(row["CourseName"]),
                        Description = Convert.ToString(row["Description"]),
                        DepartmentId = row.Table.Columns.Contains("DepartmentId") && row["DepartmentId"] != DBNull.Value
                            ? (int?)Convert.ToInt32(row["DepartmentId"])
                            : null,
                        DepartmentName = row.Table.Columns.Contains("DepartmentName")
                            ? Convert.ToString(row["DepartmentName"])
                            : string.Empty
                    });
                }

                if (Courses.Count == 0)
                {
                    _editingCourseId = 0;
                    SelectedCourse = null;
                    ResetEditorFields();
                    IsEditorActive = false;
                    IsEditorModalOpen = false;
                    StatusMessage = "No courses found. Click Add to create the first course in this workspace.";
                    return;
                }

                var match = Courses.FirstOrDefault(c => c.CourseId == selectedCourseId) ?? Courses.FirstOrDefault();
                SelectedCourse = match;
                if (!IsEditorActive)
                {
                    IsEditorModalOpen = false;
                }

                StatusMessage = "Courses loaded successfully from the current school database.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Course Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BeginAdd()
        {
            _editingCourseId = 0;
            SelectedCourse = null;
            ResetEditorFields();
            IsEditorActive = true;
            IsEditorModalOpen = true;
            StatusMessage = "Creating a new course.";
        }

        private void BeginEdit()
        {
            if (SelectedCourse == null)
            {
                return;
            }

            _editingCourseId = SelectedCourse.CourseId;
            PreviewSelected();
            IsEditorActive = true;
            IsEditorModalOpen = true;
            StatusMessage = "Editing course " + SelectedCourse.CourseCode + ".";
        }

        private void CancelEdit()
        {
            IsEditorActive = false;
            IsEditorModalOpen = false;

            if (SelectedCourse != null)
            {
                PreviewSelected();
                StatusMessage = "Edit canceled. Back to preview mode.";
            }
            else
            {
                ResetEditorFields();
                StatusMessage = "Edit canceled.";
            }
        }

        private void PreviewSelected()
        {
            if (SelectedCourse == null)
            {
                ResetEditorFields();
                _editingCourseId = 0;
                return;
            }

            _editingCourseId = SelectedCourse.CourseId;
            CourseCode = SelectedCourse.CourseCode;
            CourseName = SelectedCourse.CourseName;
            Description = SelectedCourse.Description;
            SelectedDepartment = Departments.FirstOrDefault(d => d.DepartmentId == (SelectedCourse.DepartmentId ?? 0))
                                ?? Departments.FirstOrDefault();
        }

        private void Save()
        {
            try
            {
                var departmentId = SelectedDepartment == null || SelectedDepartment.DepartmentId <= 0
                    ? (int?)null
                    : SelectedDepartment.DepartmentId;

                var course = new Course
                {
                    CourseId = _editingCourseId,
                    CourseCode = (CourseCode ?? string.Empty).Trim(),
                    CourseName = (CourseName ?? string.Empty).Trim(),
                    Description = (Description ?? string.Empty).Trim(),
                    DepartmentId = departmentId
                };

                var validation = _courseService.Validate(course);
                if (!validation.IsValid)
                {
                    MessageBox.Show(
                        validation.ToString(),
                        "Course Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                int selectedCourseId;
                string transactionAction;
                if (_editingCourseId == 0)
                {
                    selectedCourseId = _courseService.Create(course);
                    transactionAction = AppConstants.ActivityActions.Create;
                    StatusMessage = "Course saved successfully.";
                }
                else
                {
                    _courseService.Update(course);
                    selectedCourseId = _editingCourseId;
                    transactionAction = AppConstants.ActivityActions.Update;
                    StatusMessage = "Course updated successfully.";
                }

                LogCourseTransaction(
                    transactionAction,
                    selectedCourseId,
                    BuildCourseTransactionMessage(transactionAction, course));

                IsEditorActive = false;
                IsEditorModalOpen = false;
                RefreshCourses();
                SelectedCourse = Courses.FirstOrDefault(c => c.CourseId == selectedCourseId) ?? Courses.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Course Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteCurrent()
        {
            if (SelectedCourse == null)
            {
                return;
            }

            var result = MessageBox.Show(
                "Delete the selected course?",
                "Delete Course",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _courseService.Delete(SelectedCourse.CourseId);
                LogCourseTransaction(
                    AppConstants.ActivityActions.Delete,
                    SelectedCourse.CourseId,
                    "Deleted course " + BuildCourseLabel(SelectedCourse.CourseCode, SelectedCourse.CourseName) + ".");
                StatusMessage = "Course deleted successfully.";
                RefreshCourses();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Course Management",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetEditorFields()
        {
            CourseCode = string.Empty;
            CourseName = string.Empty;
            Description = string.Empty;
            SelectedDepartment = Departments.FirstOrDefault();
        }

        private void RaiseCommandStates()
        {
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private string BuildCourseTransactionMessage(string action, Course course)
        {
            var label = BuildCourseLabel(
                course == null ? null : course.CourseCode,
                course == null ? null : course.CourseName);

            if (string.Equals(action, AppConstants.ActivityActions.Create, StringComparison.OrdinalIgnoreCase))
            {
                return "Created course " + label + ".";
            }

            return "Updated course " + label + ".";
        }

        private static string BuildCourseLabel(string courseCode, string courseName)
        {
            var code = string.IsNullOrWhiteSpace(courseCode) ? "Course" : courseCode.Trim();
            var name = string.IsNullOrWhiteSpace(courseName) ? string.Empty : courseName.Trim();
            return string.IsNullOrWhiteSpace(name)
                ? code
                : code + " | " + name;
        }

        private void LogCourseTransaction(string action, int? courseId, string details)
        {
            if (_activityLogService == null)
            {
                return;
            }

            _activityLogService.LogTransaction(action, AppConstants.Entities.Course, courseId, details);
        }
    }
}
