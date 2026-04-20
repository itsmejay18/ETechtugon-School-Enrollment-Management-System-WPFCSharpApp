using System;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class DashboardHomeViewModel : ViewModelBase
    {
        private readonly AppBootstrapper _bootstrapper;
        private readonly User _currentUser;

        private string _currentDateText;
        private string _currentTimeText;
        private int _studentsCount;
        private int _facultyCount;
        private int _coursesCount;
        private int _subjectsCount;
        private int _departmentsCount;

        public DashboardHomeViewModel(AppBootstrapper bootstrapper, User currentUser)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            _currentUser = currentUser;

            SchoolBranding.BrandingChanged += SchoolBranding_BrandingChanged;
            RefreshMetrics();
            RefreshClock();
        }

        public string ApplicationTitle
        {
            get { return SchoolBranding.ApplicationTitle; }
        }

        public string ApplicationTagline
        {
            get { return SchoolBranding.ApplicationTagline; }
        }

        public string DashboardTitle
        {
            get { return SchoolBranding.DashboardTitle; }
        }

        public string DashboardSubtitle
        {
            get { return SchoolBranding.DashboardSubtitle; }
        }

        public string SerialNumber
        {
            get { return SchoolBranding.ClientSerialNumber; }
        }

        public string SupportLine
        {
            get { return SchoolBranding.SupportEmail + "  |  " + SchoolBranding.SupportPhoneNumber; }
        }

        public string CurrentUserName
        {
            get
            {
                if (_currentUser == null)
                {
                    return "Guest User";
                }

                return string.IsNullOrWhiteSpace(_currentUser.DisplayName)
                    ? _currentUser.Username
                    : _currentUser.DisplayName;
            }
        }

        public string CurrentUserRole
        {
            get
            {
                if (_currentUser == null || string.IsNullOrWhiteSpace(_currentUser.Role))
                {
                    return "User";
                }

                return _currentUser.Role;
            }
        }

        public string WorkspaceLabel
        {
            get { return CurrentUserRole + " workspace"; }
        }

        public string ConnectionModeDisplay
        {
            get
            {
                var mode = ConnectionModeHelper.GetCurrentMode("Online");
                return ConnectionModeHelper.GetDisplayName(mode);
            }
        }

        public string ConnectionSummary
        {
            get
            {
                var mode = ConnectionModeHelper.GetCurrentMode("Online");
                return ConnectionModeHelper.GetLoginSummary(mode);
            }
        }

        public string DashboardConnectionSummary
        {
            get
            {
                var mode = ConnectionModeHelper.GetDisplayName(ConnectionModeHelper.GetCurrentMode("Online"));
                return mode + " profile ready. Update database settings in Settings.";
            }
        }

        public string DashboardTileSummary
        {
            get { return "Unified WPF workspace for students, faculty, enrollment, and academics."; }
        }

        public string OperatorLine
        {
            get { return CurrentUserName + " | " + WorkspaceLabel; }
        }

        public string DatabaseStatusTitle
        {
            get { return "DATABASE CONNECTED"; }
        }

        public string DatabaseStatusDetail
        {
            get { return "School services are ready for student records, enrollment, and academic setup."; }
        }

        public string CurrentDateText
        {
            get { return _currentDateText; }
            private set { SetProperty(ref _currentDateText, value); }
        }

        public string CurrentTimeText
        {
            get { return _currentTimeText; }
            private set { SetProperty(ref _currentTimeText, value); }
        }

        public int StudentsCount
        {
            get { return _studentsCount; }
            private set
            {
                if (SetProperty(ref _studentsCount, value))
                {
                    OnPropertyChanged(nameof(TotalRecordsText));
                }
            }
        }

        public int FacultyCount
        {
            get { return _facultyCount; }
            private set
            {
                if (SetProperty(ref _facultyCount, value))
                {
                    OnPropertyChanged(nameof(TotalRecordsText));
                }
            }
        }

        public int CoursesCount
        {
            get { return _coursesCount; }
            private set
            {
                if (SetProperty(ref _coursesCount, value))
                {
                    OnPropertyChanged(nameof(TotalRecordsText));
                }
            }
        }

        public int SubjectsCount
        {
            get { return _subjectsCount; }
            private set
            {
                if (SetProperty(ref _subjectsCount, value))
                {
                    OnPropertyChanged(nameof(TotalRecordsText));
                }
            }
        }

        public int DepartmentsCount
        {
            get { return _departmentsCount; }
            private set
            {
                if (SetProperty(ref _departmentsCount, value))
                {
                    OnPropertyChanged(nameof(TotalRecordsText));
                }
            }
        }

        public string TotalRecordsText
        {
            get
            {
                var total = StudentsCount + FacultyCount + CoursesCount + SubjectsCount + DepartmentsCount;
                return total.ToString("N0") + " active school records";
            }
        }

        public void RefreshClock()
        {
            var now = DateTime.Now;
            CurrentDateText = now.ToString("dddd, d MMMM yyyy");
            CurrentTimeText = now.ToString("hh:mm:ss tt");
        }

        public void RefreshMetrics()
        {
            StudentsCount = LoadCount(() => _bootstrapper.StudentService.GetActiveCount());
            FacultyCount = LoadCount(() => _bootstrapper.FacultyService.GetActiveCount());
            CoursesCount = LoadCount(() => _bootstrapper.CourseService.GetActiveCount());
            SubjectsCount = LoadCount(() => _bootstrapper.SubjectService.GetActiveCount());
            DepartmentsCount = LoadCount(() => _bootstrapper.DepartmentService.GetActiveCount());
        }

        private static int LoadCount(Func<int> getter)
        {
            try
            {
                return Math.Max(0, getter == null ? 0 : getter());
            }
            catch
            {
                return 0;
            }
        }

        private void SchoolBranding_BrandingChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ApplicationTitle));
            OnPropertyChanged(nameof(ApplicationTagline));
            OnPropertyChanged(nameof(DashboardTitle));
            OnPropertyChanged(nameof(DashboardSubtitle));
            OnPropertyChanged(nameof(SerialNumber));
            OnPropertyChanged(nameof(SupportLine));
        }
    }
}
