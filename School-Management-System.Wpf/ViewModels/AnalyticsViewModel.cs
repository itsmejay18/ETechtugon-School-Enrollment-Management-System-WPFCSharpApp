using System;
using System.Threading.Tasks;
using System.Windows;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class AnalyticsViewModel : ViewModelBase
    {
        private readonly AppBootstrapper _bootstrapper;
        private int _studentsCount;
        private int _facultyCount;
        private int _coursesCount;
        private int _subjectsCount;
        private int _departmentsCount;
        private string _lastUpdatedText;
        private bool _hasMetricsLoaded;
        private bool _isMetricsRefreshInProgress;
        private DateTime _lastMetricsRefreshUtc;

        public AnalyticsViewModel(AppBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            RefreshCommand = new RelayCommand(() => RefreshMetrics(true));
            RefreshMetrics();
        }

        public RelayCommand RefreshCommand { get; private set; }

        public int StudentsCount
        {
            get { return _studentsCount; }
            private set { SetMetricValue(ref _studentsCount, value, nameof(StudentsCount)); }
        }

        public int FacultyCount
        {
            get { return _facultyCount; }
            private set { SetMetricValue(ref _facultyCount, value, nameof(FacultyCount)); }
        }

        public int CoursesCount
        {
            get { return _coursesCount; }
            private set { SetMetricValue(ref _coursesCount, value, nameof(CoursesCount)); }
        }

        public int SubjectsCount
        {
            get { return _subjectsCount; }
            private set { SetMetricValue(ref _subjectsCount, value, nameof(SubjectsCount)); }
        }

        public int DepartmentsCount
        {
            get { return _departmentsCount; }
            private set { SetMetricValue(ref _departmentsCount, value, nameof(DepartmentsCount)); }
        }

        public int TotalRecordsCount
        {
            get { return StudentsCount + FacultyCount + CoursesCount + SubjectsCount + DepartmentsCount; }
        }

        public string AnalyticsSummary
        {
            get { return TotalRecordsCount.ToString("N0") + " active records across the current school database."; }
        }

        public string ConnectionModeDisplay
        {
            get
            {
                var mode = ConnectionModeHelper.GetCurrentMode("Online");
                return ConnectionModeHelper.GetDisplayName(mode);
            }
        }

        public string LastUpdatedText
        {
            get { return _lastUpdatedText; }
            private set { SetProperty(ref _lastUpdatedText, value); }
        }

        public void RefreshMetrics()
        {
            RefreshMetrics(false);
        }

        private void RefreshMetrics(bool force)
        {
            if (!force && _hasMetricsLoaded && DateTime.UtcNow - _lastMetricsRefreshUtc < TimeSpan.FromSeconds(60))
            {
                return;
            }

            if (_isMetricsRefreshInProgress)
            {
                return;
            }

            _isMetricsRefreshInProgress = true;
            Task.Run(new Func<MetricsSnapshot>(LoadMetricsSnapshot))
                .ContinueWith(ApplyMetricsSnapshot);
        }

        private MetricsSnapshot LoadMetricsSnapshot()
        {
            return new MetricsSnapshot
            {
                Students = LoadCount(() => _bootstrapper.StudentService.GetActiveCount()),
                Faculty = LoadCount(() => _bootstrapper.FacultyService.GetActiveCount()),
                Courses = LoadCount(() => _bootstrapper.CourseService.GetActiveCount()),
                Subjects = LoadCount(() => _bootstrapper.SubjectService.GetActiveCount()),
                Departments = LoadCount(() => _bootstrapper.DepartmentService.GetActiveCount()),
                UpdatedAt = DateTime.Now
            };
        }

        private void ApplyMetricsSnapshot(Task<MetricsSnapshot> task)
        {
            var dispatcher = Application.Current == null ? null : Application.Current.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => ApplyMetricsSnapshot(task)));
                return;
            }

            _isMetricsRefreshInProgress = false;
            if (task == null || task.IsFaulted || task.IsCanceled || task.Result == null)
            {
                return;
            }

            var snapshot = task.Result;
            StudentsCount = snapshot.Students;
            FacultyCount = snapshot.Faculty;
            CoursesCount = snapshot.Courses;
            SubjectsCount = snapshot.Subjects;
            DepartmentsCount = snapshot.Departments;
            LastUpdatedText = "Updated " + snapshot.UpdatedAt.ToString("MMM d, yyyy h:mm tt");
            _hasMetricsLoaded = true;
            _lastMetricsRefreshUtc = DateTime.UtcNow;
        }

        private void SetMetricValue(ref int field, int value, string propertyName)
        {
            if (SetProperty(ref field, value, propertyName))
            {
                OnPropertyChanged(nameof(TotalRecordsCount));
                OnPropertyChanged(nameof(AnalyticsSummary));
            }
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

        private sealed class MetricsSnapshot
        {
            public int Students { get; set; }
            public int Faculty { get; set; }
            public int Courses { get; set; }
            public int Subjects { get; set; }
            public int Departments { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
