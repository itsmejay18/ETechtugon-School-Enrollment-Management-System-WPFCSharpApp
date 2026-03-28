using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Courses;
using School_Management_System.Models;
using System;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class ShellViewModel : ViewModelBase
    {
        private object _currentModule;
        private string _currentModuleTitle;
        private string _currentModuleSubtitle;
        private string _activeModuleKey;

        public ShellViewModel(AppBootstrapper bootstrapper, User currentUser, Action closeAction)
        {
            CurrentUser = currentUser;
            _closeAction = closeAction;

            var courseManagement = new CourseManagementViewModel(
                bootstrapper.CourseService,
                bootstrapper.DepartmentService);
            Settings = new SettingsViewModel(courseManagement, currentUser);
            Dashboard = new DashboardHomeViewModel();
            Students = new ModulePlaceholderViewModel("Students", "Students stays as a top-level navigation item, just like the WinForms dashboard.", "The WPF UI conversion for Students is next after core settings tabs.");
            Faculty = new ModulePlaceholderViewModel("Faculty", "Faculty stays as a top-level navigation item, just like the WinForms dashboard.", "The WPF UI conversion for Faculty will preserve the same list and profile workflow.");
            Enrollment = new ModulePlaceholderViewModel("Enrollment", "Enrollment stays as a top-level navigation item, just like the WinForms dashboard.", "The WPF UI conversion for Enrollment will keep the same student -> subjects -> confirm flow.");
            Schedule = new ModulePlaceholderViewModel("Schedule", "Schedule stays as a top-level navigation item, just like the WinForms dashboard.", "The WPF UI conversion will keep the same schedule and room workflow.");
            Calendar = new ModulePlaceholderViewModel("Calendar", "Calendar stays as a top-level navigation item, just like the WinForms dashboard.", "The WPF UI conversion will keep the calendar view reachable from the same sidebar.");

            ShowDashboardCommand = new RelayCommand(ShowDashboard);
            ShowStudentsCommand = new RelayCommand(ShowStudents);
            ShowFacultyCommand = new RelayCommand(ShowFaculty);
            ShowEnrollmentCommand = new RelayCommand(ShowEnrollment);
            ShowScheduleCommand = new RelayCommand(ShowSchedule);
            ShowCalendarCommand = new RelayCommand(ShowCalendar);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            LogoutCommand = new RelayCommand(Logout);

            ShowDashboard();
        }

        private readonly Action _closeAction;

        public string ApplicationTitle
        {
            get { return "School Enrollment Management System"; }
        }

        public string ApplicationSubtitle
        {
            get { return "SYSTEM"; }
        }

        public User CurrentUser { get; private set; }

        public string CurrentUserDisplay
        {
            get
            {
                if (CurrentUser == null)
                {
                    return "Guest User";
                }

                var displayName = string.IsNullOrWhiteSpace(CurrentUser.DisplayName)
                    ? CurrentUser.Username
                    : CurrentUser.DisplayName;

                return displayName + " (" + (CurrentUser.Role ?? "User") + ")";
            }
        }

        public DashboardHomeViewModel Dashboard { get; private set; }
        public ModulePlaceholderViewModel Students { get; private set; }
        public ModulePlaceholderViewModel Faculty { get; private set; }
        public ModulePlaceholderViewModel Enrollment { get; private set; }
        public ModulePlaceholderViewModel Schedule { get; private set; }
        public ModulePlaceholderViewModel Calendar { get; private set; }
        public SettingsViewModel Settings { get; private set; }

        public object CurrentModule
        {
            get { return _currentModule; }
            private set { SetProperty(ref _currentModule, value); }
        }

        public string CurrentModuleTitle
        {
            get { return _currentModuleTitle; }
            private set { SetProperty(ref _currentModuleTitle, value); }
        }

        public string CurrentModuleSubtitle
        {
            get { return _currentModuleSubtitle; }
            private set { SetProperty(ref _currentModuleSubtitle, value); }
        }

        public string ActiveModuleKey
        {
            get { return _activeModuleKey; }
            private set { SetProperty(ref _activeModuleKey, value); }
        }

        public RelayCommand ShowDashboardCommand { get; private set; }
        public RelayCommand ShowStudentsCommand { get; private set; }
        public RelayCommand ShowFacultyCommand { get; private set; }
        public RelayCommand ShowEnrollmentCommand { get; private set; }
        public RelayCommand ShowScheduleCommand { get; private set; }
        public RelayCommand ShowCalendarCommand { get; private set; }
        public RelayCommand ShowSettingsCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }

        private void ShowDashboard()
        {
            Activate("dashboard", Dashboard, "Dashboard", "The WPF shell now follows the same dashboard-first flow as the WinForms application.");
        }

        private void ShowStudents()
        {
            Activate("students", Students, "Students", "Students remains a main navigation item, matching the WinForms dashboard.");
        }

        private void ShowFaculty()
        {
            Activate("faculty", Faculty, "Faculty", "Faculty remains a main navigation item, matching the WinForms dashboard.");
        }

        private void ShowEnrollment()
        {
            Activate("enrollment", Enrollment, "Enrollment", "Enrollment remains a main navigation item, matching the WinForms dashboard.");
        }

        private void ShowSchedule()
        {
            Activate("schedule", Schedule, "Schedule", "Schedule remains a main navigation item, matching the WinForms dashboard.");
        }

        private void ShowCalendar()
        {
            Activate("calendar", Calendar, "Calendar", "Calendar remains a main navigation item, matching the WinForms dashboard.");
        }

        private void ShowSettings()
        {
            Activate("settings", Settings, "Settings", "Courses and other academic setup stay inside Settings tabs, matching the WinForms workflow.");
        }

        private void Logout()
        {
            if (_closeAction != null)
            {
                _closeAction();
            }
        }

        private void Activate(string key, object module, string title, string subtitle)
        {
            ActiveModuleKey = key;
            CurrentModule = module;
            CurrentModuleTitle = title;
            CurrentModuleSubtitle = subtitle;
        }
    }
}
