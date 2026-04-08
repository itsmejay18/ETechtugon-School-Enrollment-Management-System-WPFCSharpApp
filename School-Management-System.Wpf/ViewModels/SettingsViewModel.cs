using System;
using System.Linq;
using School_Management_System.Models;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels.Courses;
using School_Management_System.Wpf.ViewModels.Settings;
using School_Management_System.Wpf.ViewModels.Shared;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class SettingsViewModel : Infrastructure.ViewModelBase
    {
        public SettingsViewModel(AppBootstrapper bootstrapper, User currentUser)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            IsAdmin = currentUser != null &&
                      string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            CourseManagement = new CourseManagementViewModel(
                bootstrapper.CourseService,
                bootstrapper.DepartmentService);

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
        }

        public bool IsAdmin { get; private set; }
        public CourseManagementViewModel CourseManagement { get; private set; }
        public DataTableWorkspaceViewModel SystemModule { get; private set; }
        public DataTableWorkspaceViewModel DatabaseModule { get; private set; }
        public DataTableWorkspaceViewModel DepartmentsModule { get; private set; }
        public DataTableWorkspaceViewModel YearLevelsModule { get; private set; }
        public DataTableWorkspaceViewModel SectionsModule { get; private set; }
        public DataTableWorkspaceViewModel SubjectsModule { get; private set; }
        public CurriculumExplorerViewModel CurriculumModule { get; private set; }
        public DataTableWorkspaceViewModel UserManagementModule { get; private set; }
    }
}
