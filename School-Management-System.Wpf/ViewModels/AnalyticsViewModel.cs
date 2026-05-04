using System;
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

        public AnalyticsViewModel(AppBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException(nameof(bootstrapper));
            RefreshCommand = new RelayCommand(RefreshMetrics);
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
            StudentsCount = LoadCount(() => _bootstrapper.StudentService.GetActiveCount());
            FacultyCount = LoadCount(() => _bootstrapper.FacultyService.GetActiveCount());
            CoursesCount = LoadCount(() => _bootstrapper.CourseService.GetActiveCount());
            SubjectsCount = LoadCount(() => _bootstrapper.SubjectService.GetActiveCount());
            DepartmentsCount = LoadCount(() => _bootstrapper.DepartmentService.GetActiveCount());
            LastUpdatedText = "Updated " + DateTime.Now.ToString("MMM d, yyyy h:mm tt");
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
    }
}
