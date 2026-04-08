using System;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Calendar;
using School_Management_System.Wpf.ViewModels.Courses;
using School_Management_System.Wpf.ViewModels.Enrollment;
using School_Management_System.Wpf.ViewModels.Faculty;
using School_Management_System.Wpf.ViewModels.Schedule;
using School_Management_System.Wpf.ViewModels.Shared;
using School_Management_System.Wpf.ViewModels.Students;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class ShellViewModel : ViewModelBase
    {
        private readonly Action _logoutAction;
        private object _currentModule;
        private string _currentModuleTitle;
        private string _currentModuleSubtitle;
        private string _activeModuleKey;

        public ShellViewModel(AppBootstrapper bootstrapper, User currentUser, Action logoutAction)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            CurrentUser = currentUser;
            _logoutAction = logoutAction;

            Dashboard = new DashboardHomeViewModel(bootstrapper, currentUser);
            Settings = new SettingsViewModel(bootstrapper, currentUser);
            Students = new StudentDirectoryViewModel(
                bootstrapper.StudentService,
                bootstrapper.SystemSettingService);
            Faculty = new FacultyDirectoryViewModel(
                bootstrapper.FacultyService,
                bootstrapper.ClassScheduleService);
            Enrollment = new EnrollmentWorkspaceViewModel(
                bootstrapper.StudentService,
                bootstrapper.EnrollmentService,
                bootstrapper.CourseService,
                bootstrapper.LookupService,
                bootstrapper.SectionService,
                bootstrapper.CurriculumService,
                bootstrapper.ClassScheduleService,
                bootstrapper.SystemSettingService);
            Schedule = new ScheduleBoardViewModel(
                bootstrapper.SectionService,
                bootstrapper.FacultyService,
                bootstrapper.ClassScheduleService);
            Calendar = new AcademicCalendarViewModel(
                bootstrapper.LookupService,
                bootstrapper.SectionService);

            ShowDashboardCommand = new RelayCommand(ShowDashboard);
            ShowStudentsCommand = new RelayCommand(ShowStudents);
            ShowFacultyCommand = new RelayCommand(ShowFaculty);
            ShowEnrollmentCommand = new RelayCommand(ShowEnrollment);
            ShowScheduleCommand = new RelayCommand(ShowSchedule);
            ShowCalendarCommand = new RelayCommand(ShowCalendar);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            BackCommand = new RelayCommand(ShowDashboard, () => CanGoBack);
            LogoutCommand = new RelayCommand(Logout);

            ShowDashboard();
        }

        public string ApplicationTitle
        {
            get { return SchoolBranding.ApplicationTitle; }
        }

        public string ApplicationSubtitle
        {
            get { return SchoolBranding.ShellWorkspaceTagline; }
        }

        public string SupportSummary
        {
            get { return SchoolBranding.SupportEmail + "  |  " + SchoolBranding.SupportPhoneNumber; }
        }

        public string CurrentModeDisplay
        {
            get
            {
                var mode = ConnectionModeHelper.GetCurrentMode("Online");
                return ConnectionModeHelper.GetDisplayName(mode);
            }
        }

        public string CurrentModeSummary
        {
            get
            {
                var mode = ConnectionModeHelper.GetCurrentMode("Online");
                return ConnectionModeHelper.GetLoginSummary(mode);
            }
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

                return string.IsNullOrWhiteSpace(CurrentUser.DisplayName)
                    ? CurrentUser.Username
                    : CurrentUser.DisplayName;
            }
        }

        public string CurrentUserRole
        {
            get
            {
                if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.Role))
                {
                    return "User";
                }

                return CurrentUser.Role;
            }
        }

        public string WorkspaceLabel
        {
            get { return CurrentUserRole + " workspace"; }
        }

        public bool CanGoBack
        {
            get { return !string.Equals(ActiveModuleKey, "dashboard", StringComparison.OrdinalIgnoreCase); }
        }

        public DashboardHomeViewModel Dashboard { get; private set; }
        public StudentDirectoryViewModel Students { get; private set; }
        public FacultyDirectoryViewModel Faculty { get; private set; }
        public EnrollmentWorkspaceViewModel Enrollment { get; private set; }
        public ScheduleBoardViewModel Schedule { get; private set; }
        public AcademicCalendarViewModel Calendar { get; private set; }
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
        public RelayCommand BackCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }

        private void ShowDashboard()
        {
            Dashboard.RefreshMetrics();
            Dashboard.RefreshClock();
            Activate("dashboard", Dashboard, "Launch board", "Dashboard tiles, live counts, and academic workspace status.");
        }

        private void ShowStudents()
        {
            Activate("students", Students, "Students", "Student records, profiles, and enrollment entry points.");
        }

        private void ShowFaculty()
        {
            Activate("faculty", Faculty, "Faculty", "Faculty records and teaching staff management.");
        }

        private void ShowEnrollment()
        {
            Activate("enrollment", Enrollment, "Enrollment", "Enrollment intake and student subject confirmation.");
        }

        private void ShowSchedule()
        {
            Activate("schedule", Schedule, "Schedule", "Section scheduling, room usage, and faculty load.");
        }

        private void ShowCalendar()
        {
            Activate("calendar", Calendar, "Calendar", "Academic dates, reminders, and school timeline planning.");
        }

        private void ShowSettings()
        {
            Activate("settings", Settings, "Settings", "Academic setup, course maintenance, and configuration.");
        }

        private void Logout()
        {
            var action = _logoutAction;
            if (action != null)
            {
                action();
            }
        }

        private void Activate(string key, object module, string title, string subtitle)
        {
            ActiveModuleKey = key;
            CurrentModule = module;
            CurrentModuleTitle = title;
            CurrentModuleSubtitle = subtitle;
            OnPropertyChanged(nameof(CanGoBack));
            if (BackCommand != null)
            {
                BackCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
