using System;
using School_Management_System.Wpf.Infrastructure;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class ProfileWorkspaceViewModel : Infrastructure.ViewModelBase
    {
        public ProfileWorkspaceViewModel(
            string applicationTitle,
            string applicationSubtitle,
            string supportSummary,
            string currentModeDisplay,
            string currentUserDisplay,
            string currentUserRole,
            string workspaceLabel,
            Action returnToDashboard,
            Action logout)
        {
            ApplicationTitle = applicationTitle ?? string.Empty;
            ApplicationSubtitle = applicationSubtitle ?? string.Empty;
            SupportSummary = supportSummary ?? string.Empty;
            CurrentModeDisplay = currentModeDisplay ?? string.Empty;
            CurrentUserDisplay = currentUserDisplay ?? string.Empty;
            CurrentUserRole = currentUserRole ?? string.Empty;
            WorkspaceLabel = workspaceLabel ?? string.Empty;

            ReturnToDashboardCommand = new RelayCommand(returnToDashboard ?? throw new ArgumentNullException(nameof(returnToDashboard)));
            LogoutCommand = new RelayCommand(logout ?? throw new ArgumentNullException(nameof(logout)));
        }

        public string ApplicationTitle { get; private set; }
        public string ApplicationSubtitle { get; private set; }
        public string SupportSummary { get; private set; }
        public string CurrentModeDisplay { get; private set; }
        public string CurrentUserDisplay { get; private set; }
        public string CurrentUserRole { get; private set; }
        public string WorkspaceLabel { get; private set; }

        public string CurrentUserInitial
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentUserDisplay))
                {
                    return "G";
                }

                return CurrentUserDisplay.Substring(0, 1).ToUpperInvariant();
            }
        }

        public RelayCommand ReturnToDashboardCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
    }
}
