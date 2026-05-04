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
            : this(bootstrapper, currentUser, SettingsWorkspaceKind.System)
        {
        }

        public SettingsViewModel(AppBootstrapper bootstrapper, User currentUser, SettingsWorkspaceKind workspaceKind)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            WorkspaceKind = workspaceKind;
            WorkspaceTitle = workspaceKind == SettingsWorkspaceKind.Curriculum
                ? "Curriculum settings"
                : "System settings";
            WorkspaceDescription = workspaceKind == SettingsWorkspaceKind.Curriculum
                ? "Curriculum codes, course codes, subject codes, sections, and academic references are managed separately from system controls."
                : "Company branding, database activity, system controls, and user access stay together in this system workspace.";

            IsAdmin = currentUser != null &&
                      string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            Sections = new ObservableCollection<SettingsSectionItemViewModel>();

            if (workspaceKind == SettingsWorkspaceKind.Curriculum)
            {
                BuildCurriculumSections(bootstrapper);
            }
            else
            {
                BuildSystemSections(bootstrapper);
            }

            SelectedSection = Sections.FirstOrDefault();
        }

        public SettingsWorkspaceKind WorkspaceKind { get; private set; }
        public string WorkspaceTitle { get; private set; }
        public string WorkspaceDescription { get; private set; }
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
            get { return SelectedSection != null ? SelectedSection.Title : WorkspaceTitle; }
        }

        public string SelectedSectionDescription
        {
            get
            {
                return SelectedSection != null
                    ? SelectedSection.Description
                    : "Choose an area to manage its records and tools.";
            }
        }

        public object SelectedSectionModule
        {
            get { return SelectedSection != null ? SelectedSection.Module : null; }
        }

        public string SectionCountText
        {
            get
            {
                var count = Sections != null ? Sections.Count : 0;
                var label = WorkspaceKind == SettingsWorkspaceKind.Curriculum
                    ? "curriculum areas"
                    : "system areas";
                return string.Format("{0} {1}", count, label);
            }
        }

        public bool IsModalOpen
        {
            get
            {
                var modalHost = SelectedSectionModule as IModalStateHost;
                return modalHost != null && modalHost.IsModalOpen;
            }
        }

        private void BuildSystemSections(AppBootstrapper bootstrapper)
        {
            CompanyModule = new CompanyBrandingViewModel(
                bootstrapper.BrandingProfileService,
                bootstrapper.ActivityLogService);

            SystemModule = new DataTableWorkspaceViewModel(
                "System controls",
                "School-wide settings are loaded from the systemsetting table for review in the WPF shell.",
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

            UserManagementModule = new DataTableWorkspaceViewModel(
                "User management",
                IsAdmin
                    ? "User accounts are loaded from the users table for read-only review inside the WPF shell."
                    : "User account records are visible here in read-only mode for this session.",
                "Search usernames or roles",
                bootstrapper.UserManagementService.GetUsers);
            UserManagementModule.Refresh();

            Sections.Add(new SettingsSectionItemViewModel("Company", "Branding logos, landing-form copy, and shell identity stored in MySQL.", "\uE8D1", CompanyModule));
            Sections.Add(new SettingsSectionItemViewModel("System", "School-wide controls, system variables, and operating preferences.", "\uE713", SystemModule));
            Sections.Add(new SettingsSectionItemViewModel("Database", "Activity history, connection records, and database monitoring details.", "\uE9F9", DatabaseModule));
            Sections.Add(new SettingsSectionItemViewModel("User Management", "Read-only account review for operators and administrators.", "\uE716", UserManagementModule));
        }

        private void BuildCurriculumSections(AppBootstrapper bootstrapper)
        {
            CourseManagement = new CourseManagementViewModel(
                bootstrapper.CourseService,
                bootstrapper.DepartmentService,
                bootstrapper.ActivityLogService);

            DepartmentsModule = new DataTableWorkspaceViewModel(
                "Department codes",
                "Department code records and organizational structure are loaded from the active academic database.",
                "Search department codes",
                bootstrapper.DepartmentService.GetDepartments);
            DepartmentsModule.Refresh();

            YearLevelsModule = new DataTableWorkspaceViewModel(
                "Year levels",
                "Year-level references used by enrollment and scheduling are loaded from the active academic database.",
                "Search year levels",
                bootstrapper.YearLevelService.GetYearLevels);
            YearLevelsModule.Refresh();

            SectionsModule = new DataTableWorkspaceViewModel(
                "Sections",
                "Section assignments, labels, and active class groups are loaded from the active academic database.",
                "Search sections",
                bootstrapper.SectionService.GetSections);
            SectionsModule.Refresh();

            SubjectsModule = new DataTableWorkspaceViewModel(
                "Subject codes",
                "Subject code catalog rows are loaded from the live school database.",
                "Search subject codes",
                bootstrapper.SubjectService.GetSubjects);
            SubjectsModule.Refresh();

            CurriculumModule = new CurriculumExplorerViewModel(
                bootstrapper.CurriculumService,
                bootstrapper.CourseService,
                bootstrapper.LookupService);

            Sections.Add(new SettingsSectionItemViewModel("Departments", "Academic department codes and organizational structure.", "\uE7EF", DepartmentsModule));
            Sections.Add(new SettingsSectionItemViewModel("Courses", "Course code maintenance, validation, and editing workspace.", "\uE82D", CourseManagement));
            Sections.Add(new SettingsSectionItemViewModel("Year Levels", "Year-level references used by enrollment and scheduling.", "\uE8D2", YearLevelsModule));
            Sections.Add(new SettingsSectionItemViewModel("Sections", "Section labels, class groups, and academic grouping references.", "\uE8C8", SectionsModule));
            Sections.Add(new SettingsSectionItemViewModel("Subjects", "Subject code catalog management for the live school database.", "\uE8EF", SubjectsModule));
            Sections.Add(new SettingsSectionItemViewModel("Curriculum Codes", "Curriculum code explorer for course, year, semester, and subject mappings.", "\uE7BE", CurriculumModule));
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

    public enum SettingsWorkspaceKind
    {
        System,
        Curriculum
    }
}
