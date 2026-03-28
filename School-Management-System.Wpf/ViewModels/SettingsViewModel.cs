using School_Management_System.Models;
using School_Management_System.Wpf.ViewModels.Courses;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class SettingsViewModel : Infrastructure.ViewModelBase
    {
        public SettingsViewModel(CourseManagementViewModel courseManagement, User currentUser)
        {
            CourseManagement = courseManagement;
            IsAdmin = currentUser != null &&
                      string.Equals(currentUser.Role, "Admin", System.StringComparison.OrdinalIgnoreCase);

            SystemModule = new ModulePlaceholderViewModel(
                "System",
                "Global term settings stay in Settings, just like the WinForms app.",
                "This tab will be converted next in WPF without moving it out of the Settings module.");

            DatabaseModule = new ModulePlaceholderViewModel(
                "Database",
                IsAdmin
                    ? "Database connection profiles, backup tools, and logs remain under Settings."
                    : "Database configuration remains hidden from regular users.",
                IsAdmin
                    ? "This will stay admin-only in WPF, matching the WinForms behavior."
                    : "Only administrators can view or edit database settings.");

            DepartmentsModule = new ModulePlaceholderViewModel(
                "Departments",
                "Departments will remain a tab inside Settings.",
                "UI conversion pending. Backend logic stays unchanged.");

            YearLevelsModule = new ModulePlaceholderViewModel(
                "Year Levels",
                "Year Levels will remain a tab inside Settings.",
                "UI conversion pending. Backend logic stays unchanged.");

            SectionsModule = new ModulePlaceholderViewModel(
                "Sections",
                "Sections will remain a tab inside Settings.",
                "UI conversion pending. Backend logic stays unchanged.");

            SubjectsModule = new ModulePlaceholderViewModel(
                "Subjects",
                "Subjects will remain a tab inside Settings.",
                "UI conversion pending. Backend logic stays unchanged.");

            CurriculumModule = new ModulePlaceholderViewModel(
                "Curriculum",
                "Curriculum will remain a tab inside Settings.",
                "UI conversion pending. Backend logic stays unchanged.");

            UserManagementModule = new ModulePlaceholderViewModel(
                "User Management",
                IsAdmin
                    ? "User management stays under Settings for administrators."
                    : "Only administrators can manage users and passwords.",
                "UI conversion pending. Backend logic stays unchanged.");
        }

        public bool IsAdmin { get; private set; }
        public CourseManagementViewModel CourseManagement { get; private set; }
        public ModulePlaceholderViewModel SystemModule { get; private set; }
        public ModulePlaceholderViewModel DatabaseModule { get; private set; }
        public ModulePlaceholderViewModel DepartmentsModule { get; private set; }
        public ModulePlaceholderViewModel YearLevelsModule { get; private set; }
        public ModulePlaceholderViewModel SectionsModule { get; private set; }
        public ModulePlaceholderViewModel SubjectsModule { get; private set; }
        public ModulePlaceholderViewModel CurriculumModule { get; private set; }
        public ModulePlaceholderViewModel UserManagementModule { get; private set; }
    }
}
