using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
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
        private readonly ActivityLogService _activityLogService;
        private INotifyPropertyChanged _modalHostNotifier;
        private object _currentModule;
        private string _currentModuleTitle;
        private string _currentModuleSubtitle;
        private string _activeModuleKey;
        private bool _isNotificationModalOpen;

        public ShellViewModel(AppBootstrapper bootstrapper, User currentUser, Action logoutAction)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            CurrentUser = currentUser;
            _logoutAction = logoutAction;
            _activityLogService = bootstrapper.ActivityLogService;

            Dashboard = new DashboardHomeViewModel(bootstrapper, currentUser);
            Settings = new SettingsViewModel(bootstrapper, currentUser);
            Students = new StudentDirectoryViewModel(
                bootstrapper.StudentService,
                bootstrapper.SystemSettingService,
                bootstrapper.ActivityLogService);
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
                bootstrapper.SystemSettingService,
                bootstrapper.ActivityLogService);
            Schedule = new ScheduleBoardViewModel(
                bootstrapper.SectionService,
                bootstrapper.FacultyService,
                bootstrapper.ClassScheduleService);
            Calendar = new AcademicCalendarViewModel(
                bootstrapper.LookupService,
                bootstrapper.SectionService);
            Profile = new ProfileWorkspaceViewModel(
                SchoolBranding.ApplicationTitle,
                SchoolBranding.ShellWorkspaceTagline,
                SchoolBranding.SupportEmail + "  |  " + SchoolBranding.SupportPhoneNumber,
                CurrentModeDisplay,
                CurrentUserDisplay,
                CurrentUserRole,
                WorkspaceLabel,
                ShowDashboard,
                Logout);
            Notifications = new ObservableCollection<NotificationItemViewModel>();

            ShowDashboardCommand = new RelayCommand(ShowDashboard);
            ShowStudentsCommand = new RelayCommand(ShowStudents);
            ShowFacultyCommand = new RelayCommand(ShowFaculty);
            ShowEnrollmentCommand = new RelayCommand(ShowEnrollment);
            ShowScheduleCommand = new RelayCommand(ShowSchedule);
            ShowCalendarCommand = new RelayCommand(ShowCalendar);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowProfileCommand = new RelayCommand(ShowProfile);
            ShowNotificationsCommand = new RelayCommand(ToggleNotifications);
            CloseNotificationsCommand = new RelayCommand(CloseNotifications);
            BackCommand = new RelayCommand(ShowDashboard, () => CanGoBack);
            LogoutCommand = new RelayCommand(Logout);

            SchoolBranding.BrandingChanged += SchoolBranding_BrandingChanged;
            if (_activityLogService != null)
            {
                _activityLogService.ActivityLogged += ActivityLogService_ActivityLogged;
            }
            RefreshNotifications();
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

        public string CurrentUserInitial
        {
            get
            {
                var text = CurrentUserDisplay;
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "G";
                }

                return text.Substring(0, 1).ToUpperInvariant();
            }
        }

        public string CurrentUserAccountSummary
        {
            get { return CurrentUserDisplay + " | " + CurrentUserRole; }
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
        public ProfileWorkspaceViewModel Profile { get; private set; }
        public ObservableCollection<NotificationItemViewModel> Notifications { get; private set; }

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
        public RelayCommand ShowProfileCommand { get; private set; }
        public RelayCommand ShowNotificationsCommand { get; private set; }
        public RelayCommand CloseNotificationsCommand { get; private set; }
        public RelayCommand BackCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }

        public bool IsNotificationModalOpen
        {
            get { return _isNotificationModalOpen; }
            private set
            {
                if (SetProperty(ref _isNotificationModalOpen, value))
                {
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            }
        }

        public int NotificationCount
        {
            get { return Notifications == null ? 0 : Notifications.Count; }
        }

        public bool HasNotifications
        {
            get { return NotificationCount > 0; }
        }

        public string NotificationBadgeText
        {
            get
            {
                if (NotificationCount <= 0)
                {
                    return string.Empty;
                }

                return NotificationCount > 9
                    ? "9+"
                    : NotificationCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        public bool IsModalOpen
        {
            get
            {
                var modalHost = CurrentModule as IModalStateHost;
                return IsNotificationModalOpen || (modalHost != null && modalHost.IsModalOpen);
            }
        }

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

        private void ShowProfile()
        {
            Activate("profile", Profile, "Profile", "Signed-in account details and session information.");
        }

        private void ToggleNotifications()
        {
            if (!IsNotificationModalOpen)
            {
                RefreshNotifications();
            }

            IsNotificationModalOpen = !IsNotificationModalOpen;
        }

        private void CloseNotifications()
        {
            IsNotificationModalOpen = false;
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
            DetachModalHost();
            ActiveModuleKey = key;
            CurrentModule = module;
            CurrentModuleTitle = title;
            CurrentModuleSubtitle = subtitle;
            AttachModalHost(module);
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(IsModalOpen));
            if (BackCommand != null)
            {
                BackCommand.RaiseCanExecuteChanged();
            }
        }

        private void RefreshNotifications()
        {
            Notifications.Clear();

            if (_activityLogService == null)
            {
                RaiseNotificationState();
                return;
            }

            try
            {
                var logs = _activityLogService.Search(null, null, null, null, 80);
                for (var i = 0; i < logs.Count && Notifications.Count < 12; i++)
                {
                    if (!IsTransactionLog(logs[i]))
                    {
                        continue;
                    }

                    var item = CreateNotificationItem(logs[i]);
                    if (item != null)
                    {
                        Notifications.Add(item);
                    }
                }
            }
            catch
            {
            }

            RaiseNotificationState();
        }

        private static NotificationItemViewModel CreateNotificationItem(ActivityLog log)
        {
            if (log == null)
            {
                return null;
            }

            var title = BuildNotificationTitle(log);
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Transaction";
            }

            var actor = string.IsNullOrWhiteSpace(log.DisplayName)
                ? (string.IsNullOrWhiteSpace(log.Username) ? "System user" : log.Username)
                : log.DisplayName;

            var subtitle = string.IsNullOrWhiteSpace(log.Username) || string.Equals(actor, log.Username, StringComparison.OrdinalIgnoreCase)
                ? actor
                : actor + " • " + log.Username;

            var timestamp = DateTime.SpecifyKind(log.CreatedAt, DateTimeKind.Utc).ToLocalTime();

            return new NotificationItemViewModel(
                title,
                subtitle,
                string.IsNullOrWhiteSpace(log.Details) ? "Recent transaction activity." : log.Details,
                FormatRelativeTime(timestamp));
        }

        private static bool IsTransactionLog(ActivityLog log)
        {
            if (log == null || string.IsNullOrWhiteSpace(log.Entity) || string.IsNullOrWhiteSpace(log.Action))
            {
                return false;
            }

            var isKnownEntity =
                string.Equals(log.Entity, AppConstants.Entities.Student, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(log.Entity, AppConstants.Entities.Course, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(log.Entity, AppConstants.Entities.Enrollment, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(log.Entity, AppConstants.Entities.BrandingProfile, StringComparison.OrdinalIgnoreCase);

            if (!isKnownEntity)
            {
                return false;
            }

            return string.Equals(log.Action, AppConstants.ActivityActions.Create, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(log.Action, AppConstants.ActivityActions.Update, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(log.Action, AppConstants.ActivityActions.Delete, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(log.Action, AppConstants.ActivityActions.Post, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(log.Action, AppConstants.ActivityActions.Save, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildNotificationTitle(ActivityLog log)
        {
            if (log == null)
            {
                return string.Empty;
            }

            var entityName = GetNotificationEntityName(log.Entity);
            if (string.IsNullOrWhiteSpace(entityName))
            {
                entityName = "Transaction";
            }

            if (string.Equals(log.Action, AppConstants.ActivityActions.Create, StringComparison.OrdinalIgnoreCase))
            {
                return entityName + " created";
            }

            if (string.Equals(log.Action, AppConstants.ActivityActions.Update, StringComparison.OrdinalIgnoreCase))
            {
                return entityName + " updated";
            }

            if (string.Equals(log.Action, AppConstants.ActivityActions.Delete, StringComparison.OrdinalIgnoreCase))
            {
                return entityName + " deleted";
            }

            if (string.Equals(log.Action, AppConstants.ActivityActions.Post, StringComparison.OrdinalIgnoreCase))
            {
                return entityName + " posted";
            }

            if (string.Equals(log.Action, AppConstants.ActivityActions.Save, StringComparison.OrdinalIgnoreCase))
            {
                return entityName + " saved";
            }

            return entityName;
        }

        private static string GetNotificationEntityName(string entity)
        {
            if (string.IsNullOrWhiteSpace(entity))
            {
                return string.Empty;
            }

            if (string.Equals(entity, AppConstants.Entities.BrandingProfile, StringComparison.OrdinalIgnoreCase))
            {
                return "Company profile";
            }

            if (string.Equals(entity, AppConstants.Entities.SystemSetting, StringComparison.OrdinalIgnoreCase))
            {
                return "System setting";
            }

            return entity;
        }

        private static string FormatRelativeTime(DateTime timestamp)
        {
            var delta = DateTime.Now - timestamp;
            if (delta.TotalMinutes < 1)
            {
                return "Now";
            }

            if (delta.TotalHours < 1)
            {
                return Math.Max(1, (int)delta.TotalMinutes).ToString(CultureInfo.InvariantCulture) + "m";
            }

            if (delta.TotalDays < 1)
            {
                return Math.Max(1, (int)delta.TotalHours).ToString(CultureInfo.InvariantCulture) + "h";
            }

            return Math.Max(1, (int)delta.TotalDays).ToString(CultureInfo.InvariantCulture) + "d";
        }

        private void RaiseNotificationState()
        {
            OnPropertyChanged(nameof(NotificationCount));
            OnPropertyChanged(nameof(HasNotifications));
            OnPropertyChanged(nameof(NotificationBadgeText));
        }

        private void SchoolBranding_BrandingChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ApplicationTitle));
            OnPropertyChanged(nameof(ApplicationSubtitle));
            OnPropertyChanged(nameof(SupportSummary));
            OnPropertyChanged(nameof(CurrentUserAccountSummary));
        }

        private void AttachModalHost(object module)
        {
            _modalHostNotifier = module as INotifyPropertyChanged;
            if (_modalHostNotifier != null)
            {
                _modalHostNotifier.PropertyChanged += ModalHost_PropertyChanged;
            }
        }

        private void DetachModalHost()
        {
            if (_modalHostNotifier != null)
            {
                _modalHostNotifier.PropertyChanged -= ModalHost_PropertyChanged;
                _modalHostNotifier = null;
            }
        }

        private void ModalHost_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) ||
                string.Equals(e.PropertyName, nameof(IModalStateHost.IsModalOpen), StringComparison.Ordinal))
            {
                OnPropertyChanged(nameof(IsModalOpen));
            }
        }

        private void ActivityLogService_ActivityLogged(object sender, ActivityLogService.ActivityLoggedEventArgs e)
        {
            if (e == null || !IsTransactionLog(e.Entry))
            {
                return;
            }

            var dispatcher = Application.Current == null ? null : Application.Current.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(RefreshNotifications));
                return;
            }

            RefreshNotifications();
        }

        public sealed class NotificationItemViewModel
        {
            public NotificationItemViewModel(string title, string subtitle, string message, string relativeTime)
            {
                Title = title ?? string.Empty;
                Subtitle = subtitle ?? string.Empty;
                Message = message ?? string.Empty;
                RelativeTime = relativeTime ?? string.Empty;
            }

            public string Title { get; private set; }
            public string Subtitle { get; private set; }
            public string Message { get; private set; }
            public string RelativeTime { get; private set; }
        }
    }
}
