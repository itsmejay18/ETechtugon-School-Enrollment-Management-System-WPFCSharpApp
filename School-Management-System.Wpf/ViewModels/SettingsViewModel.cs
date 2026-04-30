using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using School_Management_System.Models;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Courses;
using School_Management_System.Wpf.ViewModels.Settings;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class SettingsViewModel : Infrastructure.ViewModelBase, IModalStateHost
    {
        private INotifyPropertyChanged _selectedSectionModuleNotifier;

        public SettingsViewModel(AppBootstrapper bootstrapper, User currentUser)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            IsAdmin = currentUser != null &&
                      string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            CourseManagement = new CourseManagementViewModel(
                bootstrapper.CourseService,
                bootstrapper.DepartmentService,
                bootstrapper.ActivityLogService);
            CompanyModule = new CompanyBrandingViewModel(
                bootstrapper.BrandingProfileService,
                bootstrapper.ActivityLogService);

            SystemModule = new DataTableWorkspaceViewModel(
                "System settings",
                "School-wide settings are now loaded from the systemsetting table instead of a placeholder card.",
                "Search setting key or value",
                search => WpfUiDataHelper.CreateSettingsTable(bootstrapper.SystemSettingService.GetAll(), search));
            SystemModule.Refresh();

            DatabaseModule = new DataTableWorkspaceViewModel(
                "Database activity",
                "Recent audit and connection activity is loaded directly from the activity log service.",
                "Search username or action",
                search => WpfUiDataHelper.CreateActivityLogTable(
                    bootstrapper.ActivityLogService.Search(null, null, search, null, 150)));
            DatabaseModule.Refresh();

            DepartmentsModule = new DataTableWorkspaceViewModel(
                "Departments",
                "Department records are loaded from the active academic database.",
                "Search departments",
                bootstrapper.DepartmentService.GetDepartments);
            DepartmentsModule.Refresh();

            YearLevelsModule = new DataTableWorkspaceViewModel(
                "Year levels",
                "Year level records are loaded from the active academic database.",
                "Search year levels",
                bootstrapper.YearLevelService.GetYearLevels);
            YearLevelsModule.Refresh();

            SectionsModule = new DataTableWorkspaceViewModel(
                "Sections",
                "Section records are loaded from the active academic database.",
                "Search sections",
                bootstrapper.SectionService.GetSections);
            SectionsModule.Refresh();

            SubjectsModule = new DataTableWorkspaceViewModel(
                "Subjects",
                "Subject catalog rows are loaded from the live database.",
                "Search subjects",
                bootstrapper.SubjectService.GetSubjects);
            SubjectsModule.Refresh();

            CurriculumModule = new CurriculumExplorerViewModel(
                bootstrapper.CurriculumService,
                bootstrapper.CourseService,
                bootstrapper.LookupService);

            UserManagementModule = new DataTableWorkspaceViewModel(
                "User management",
                IsAdmin
                    ? "User accounts are loaded from the users table for read-only review inside the WPF shell."
                    : "User account records are visible here in read-only mode for this session.",
                "Search usernames or roles",
                bootstrapper.UserManagementService.GetUsers);
            UserManagementModule.Refresh();

            Sections = new ObservableCollection<SettingsSectionItemViewModel>
            {
                new SettingsSectionItemViewModel("Company", "Branding logos, landing-form copy, and shell identity stored in MySQL.", "\uE8D1", CompanyModule),
                new SettingsSectionItemViewModel("System", "School-wide controls, system variables, and operating preferences.", "\uE713", SystemModule),
                new SettingsSectionItemViewModel("Database", "Activity history, connection records, and database monitoring details.", "\uE9F9", DatabaseModule),
                new SettingsSectionItemViewModel("Departments", "Academic department records and organizational structure.", "\uE7EF", DepartmentsModule),
                new SettingsSectionItemViewModel("Courses", "Program maintenance, validation, and course editing workspace.", "\uE82D", CourseManagement),
                new SettingsSectionItemViewModel("Year Levels", "Year-level references used by enrollment and scheduling.", "\uE8D2", YearLevelsModule),
                new SettingsSectionItemViewModel("Sections", "Section assignments, labels, and active class groups.", "\uE8C8", SectionsModule),
                new SettingsSectionItemViewModel("Subjects", "Subject catalog management for the live school database.", "\uE8EF", SubjectsModule),
                new SettingsSectionItemViewModel("Curriculum", "Curriculum explorer for course, year, and semester mappings.", "\uE7BE", CurriculumModule),
                new SettingsSectionItemViewModel("User Management", "Read-only account review for operators and administrators.", "\uE716", UserManagementModule)
            };

            SelectedSection = Sections.FirstOrDefault();
        }

        public bool IsAdmin { get; private set; }
        public CourseManagementViewModel CourseManagement { get; private set; }
        public CompanyBrandingViewModel CompanyModule { get; private set; }
        public DataTableWorkspaceViewModel SystemModule { get; private set; }
        public DataTableWorkspaceViewModel DatabaseModule { get; private set; }
        public DataTableWorkspaceViewModel DepartmentsModule { get; private set; }
        public DataTableWorkspaceViewModel YearLevelsModule { get; private set; }
        public DataTableWorkspaceViewModel SectionsModule { get; private set; }
        public DataTableWorkspaceViewModel SubjectsModule { get; private set; }
        public CurriculumExplorerViewModel CurriculumModule { get; private set; }
        public DataTableWorkspaceViewModel UserManagementModule { get; private set; }
        public ObservableCollection<SettingsSectionItemViewModel> Sections { get; private set; }

        private SettingsSectionItemViewModel _selectedSection;
        public SettingsSectionItemViewModel SelectedSection
        {
            get { return _selectedSection; }
            set
            {
                var previousModule = _selectedSection != null ? _selectedSection.Module : null;
                if (SetProperty(ref _selectedSection, value))
                {
                    UpdateSelectedSectionModuleSubscription(previousModule, SelectedSectionModule);
                    OnPropertyChanged(nameof(SelectedSectionTitle));
                    OnPropertyChanged(nameof(SelectedSectionDescription));
                    OnPropertyChanged(nameof(SelectedSectionModule));
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            }
        }

        public string SelectedSectionTitle
        {
            get { return SelectedSection != null ? SelectedSection.Title : "Settings"; }
        }

        public string SelectedSectionDescription
        {
            get
            {
                return SelectedSection != null
                    ? SelectedSection.Description
                    : "Choose a settings area to manage its records and tools.";
            }
        }

        public object SelectedSectionModule
        {
            get { return SelectedSection != null ? SelectedSection.Module : null; }
        }

        public string SectionCountText
        {
            get { return string.Format("{0} settings areas", Sections != null ? Sections.Count : 0); }
        }

        public bool IsModalOpen
        {
            get
            {
                var modalHost = SelectedSectionModule as IModalStateHost;
                return modalHost != null && modalHost.IsModalOpen;
            }
        }

        private void UpdateSelectedSectionModuleSubscription(object previousModule, object nextModule)
        {
            var oldNotifier = previousModule as INotifyPropertyChanged;
            if (oldNotifier != null)
            {
                oldNotifier.PropertyChanged -= SelectedSectionModule_PropertyChanged;
            }

            _selectedSectionModuleNotifier = nextModule as INotifyPropertyChanged;
            if (_selectedSectionModuleNotifier != null)
            {
                _selectedSectionModuleNotifier.PropertyChanged += SelectedSectionModule_PropertyChanged;
            }
        }

        private void SelectedSectionModule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName) ||
                string.Equals(e.PropertyName, nameof(IModalStateHost.IsModalOpen), StringComparison.Ordinal))
            {
                OnPropertyChanged(nameof(IsModalOpen));
            }
        }
    }
}
