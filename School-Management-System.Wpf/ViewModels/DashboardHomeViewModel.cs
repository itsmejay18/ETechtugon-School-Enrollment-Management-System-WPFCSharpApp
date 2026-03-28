namespace School_Management_System.Wpf.ViewModels
{
    public sealed class DashboardHomeViewModel : Infrastructure.ViewModelBase
    {
        public string WelcomeTitle
        {
            get { return "Dashboard"; }
        }

        public string WelcomeBody
        {
            get { return "This WPF dashboard now follows the same main flow as the WinForms app: navigate from the sidebar, then manage academic setup through Settings tabs."; }
        }

        public string[] ModuleCards
        {
            get
            {
                return new[]
                {
                    "Students",
                    "Faculty",
                    "Enrollment",
                    "Schedule",
                    "Calendar",
                    "Settings"
                };
            }
        }
    }
}
