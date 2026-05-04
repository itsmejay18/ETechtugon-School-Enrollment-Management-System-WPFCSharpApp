using System;
using System.Collections.ObjectModel;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels
{
    public sealed class LoginViewModel : ViewModelBase
    {
        private readonly LoginConnectionService _connectionService;

        private string _username;
        private string _statusMessage;
        private QuickLoginOption _selectedQuickLogin;
        private ConnectionProfileOption _selectedConnectionProfile;
        private string _connectionStatusTitle;
        private string _connectionStatusMessage;
        private bool _isConnectionReady;
        private bool _hasConnectionFeedback;
        private bool _isStatusError;
        private bool _isStatusSuccess;

        public LoginViewModel(LoginConnectionService connectionService)
        {
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));

            QuickLoginOptions = new ObservableCollection<QuickLoginOption>
            {
                new QuickLoginOption("Select account...", string.Empty, string.Empty),
                new QuickLoginOption("Admin", "admin", "admin123"),
                new QuickLoginOption("Faculty", "faculty1", "faculty123"),
                new QuickLoginOption("Registrar", "registrar", "registrar123")
            };

            ConnectionProfiles = new ObservableCollection<ConnectionProfileOption>();
            CancelCommand = new RelayCommand(() => RequestClose(false));
            RefreshConnectionCommand = new RelayCommand(RefreshConnectionStatus);

            StatusMessage = string.Empty;
            LoadConnectionProfiles();
            SelectedQuickLogin = QuickLoginOptions.Count > 1 ? QuickLoginOptions[1] : QuickLoginOptions[0];
        }

        public event Action<bool?> CloseRequested;

        public ObservableCollection<QuickLoginOption> QuickLoginOptions { get; private set; }
        public ObservableCollection<ConnectionProfileOption> ConnectionProfiles { get; private set; }

        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand RefreshConnectionCommand { get; private set; }

        public User AuthenticatedUser { get; private set; }

        public string ClientSerialNumber
        {
            get { return SchoolBranding.ClientSerialNumber; }
        }

        public string SupportGmail
        {
            get { return SchoolBranding.SupportEmail; }
        }

        public string SupportPhoneNumber
        {
            get { return SchoolBranding.SupportPhoneNumber; }
        }

        public string ApplicationTitle
        {
            get { return SchoolBranding.ApplicationTitle; }
        }

        public string ApplicationTagline
        {
            get { return SchoolBranding.ApplicationTagline; }
        }

        public string LoginHeadline
        {
            get { return SchoolBranding.LoginHeadline; }
        }

        public string LoginBody
        {
            get { return SchoolBranding.LoginBody; }
        }

        public string LoginFormTitle
        {
            get { return SchoolBranding.LoginFormTitle; }
        }

        public string LoginFormSubtitle
        {
            get { return SchoolBranding.LoginFormSubtitle; }
        }

        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set
            {
                if (SetProperty(ref _statusMessage, value))
                {
                    OnPropertyChanged(nameof(HasStatusMessage));
                }
            }
        }

        public bool HasStatusMessage
        {
            get { return !string.IsNullOrWhiteSpace(StatusMessage); }
        }

        public bool IsStatusError
        {
            get { return _isStatusError; }
            private set { SetProperty(ref _isStatusError, value); }
        }

        public bool IsStatusSuccess
        {
            get { return _isStatusSuccess; }
            private set { SetProperty(ref _isStatusSuccess, value); }
        }

        public QuickLoginOption SelectedQuickLogin
        {
            get { return _selectedQuickLogin; }
            set
            {
                if (SetProperty(ref _selectedQuickLogin, value) && value != null && !string.IsNullOrWhiteSpace(value.Username))
                {
                    Username = value.Username;
                }
            }
        }

        public ConnectionProfileOption SelectedConnectionProfile
        {
            get { return _selectedConnectionProfile; }
            set
            {
                if (SetProperty(ref _selectedConnectionProfile, value) && value != null)
                {
                    HasConnectionFeedback = false;
                    ConnectionStatusTitle = string.Empty;
                    ConnectionStatusMessage = string.Empty;
                    IsConnectionReady = false;
                    OnPropertyChanged(nameof(SelectedConnectionCaption));
                    OnPropertyChanged(nameof(SelectedConnectionDetails));
                    LoadBrandingForSelectedConnection();
                }
            }
        }

        public string ConnectionStatusTitle
        {
            get { return _connectionStatusTitle; }
            private set { SetProperty(ref _connectionStatusTitle, value); }
        }

        public string ConnectionStatusMessage
        {
            get { return _connectionStatusMessage; }
            private set { SetProperty(ref _connectionStatusMessage, value); }
        }

        public bool IsConnectionReady
        {
            get { return _isConnectionReady; }
            private set { SetProperty(ref _isConnectionReady, value); }
        }

        public bool HasConnectionFeedback
        {
            get { return _hasConnectionFeedback; }
            private set { SetProperty(ref _hasConnectionFeedback, value); }
        }

        public string SelectedConnectionCaption
        {
            get
            {
                if (SelectedConnectionProfile == null)
                {
                    return string.Empty;
                }

                return SelectedConnectionProfile.Caption;
            }
        }

        public string SelectedConnectionDetails
        {
            get
            {
                if (SelectedConnectionProfile == null || SelectedConnectionProfile.Profile == null)
                {
                    return string.Empty;
                }

                var profile = SelectedConnectionProfile.Profile;
                return "Server: " + profile.Host + ":" + profile.Port + "   Database: " + profile.Database;
            }
        }

        public bool TryLogin(string password)
        {
            if (SelectedConnectionProfile == null)
            {
                SetStatus("Select a database connection profile first.", true, false);
                return false;
            }

            User user;
            string message;
            if (!_connectionService.TryLogin(SelectedConnectionProfile.Profile, Username, password, out user, out message))
            {
                RefreshConnectionStatus();
                SetStatus(string.IsNullOrWhiteSpace(message) ? "Login failed. Check your credentials." : message, true, false);
                return false;
            }

            AuthenticatedUser = user;
            SetStatus("Login successful.", false, true);
            RequestClose(true);
            return true;
        }

        public void RefreshProfiles()
        {
            LoadConnectionProfiles();
        }

        private void LoadConnectionProfiles()
        {
            var currentMode = _connectionService.GetCurrentMode();
            ConnectionProfiles.Clear();

            var profiles = _connectionService.GetProfiles();
            ConnectionProfileOption selected = null;
            for (var i = 0; i < profiles.Count; i++)
            {
                var option = new ConnectionProfileOption(profiles[i]);
                ConnectionProfiles.Add(option);

                if (selected == null && string.Equals(option.Mode, currentMode, StringComparison.OrdinalIgnoreCase))
                {
                    selected = option;
                }
            }

            if (selected == null && ConnectionProfiles.Count > 0)
            {
                selected = ConnectionProfiles[0];
            }

            SelectedConnectionProfile = selected;
            if (selected == null)
            {
                LoadBrandingForSelectedConnection();
            }
        }

        private void RefreshConnectionStatus()
        {
            if (SelectedConnectionProfile == null)
            {
                ConnectionStatusTitle = "Connection not selected";
                ConnectionStatusMessage = "Choose a database profile before signing in.";
                IsConnectionReady = false;
                return;
            }

            var probe = _connectionService.ApplyAndTest(SelectedConnectionProfile.Profile);
            ConnectionStatusTitle = probe.Title;
            ConnectionStatusMessage = probe.Message;
            IsConnectionReady = probe.IsSuccess;
            HasConnectionFeedback = true;
        }

        private void SetStatus(string message, bool isError, bool isSuccess)
        {
            StatusMessage = message;
            IsStatusError = isError;
            IsStatusSuccess = isSuccess;
        }

        private void LoadBrandingForSelectedConnection()
        {
            _connectionService.LoadBranding(SelectedConnectionProfile == null ? null : SelectedConnectionProfile.Profile);

            OnPropertyChanged(nameof(ClientSerialNumber));
            OnPropertyChanged(nameof(SupportGmail));
            OnPropertyChanged(nameof(SupportPhoneNumber));
            OnPropertyChanged(nameof(ApplicationTitle));
            OnPropertyChanged(nameof(ApplicationTagline));
            OnPropertyChanged(nameof(LoginHeadline));
            OnPropertyChanged(nameof(LoginBody));
            OnPropertyChanged(nameof(LoginFormTitle));
            OnPropertyChanged(nameof(LoginFormSubtitle));
        }

        private void RequestClose(bool? result)
        {
            var handler = CloseRequested;
            if (handler != null)
            {
                handler(result);
            }
        }

        public sealed class QuickLoginOption
        {
            public QuickLoginOption(string label, string username, string password)
            {
                Label = label ?? string.Empty;
                Username = username ?? string.Empty;
                Password = password ?? string.Empty;
            }

            public string Label { get; private set; }
            public string Username { get; private set; }
            public string Password { get; private set; }

            public override string ToString()
            {
                return Label;
            }
        }

        public sealed class ConnectionProfileOption
        {
            public ConnectionProfileOption(LoginConnectionService.ConnectionProfile profile)
            {
                Profile = profile ?? throw new ArgumentNullException(nameof(profile));
            }

            public LoginConnectionService.ConnectionProfile Profile { get; private set; }
            public string Mode { get { return Profile.Mode; } }
            public string DisplayName { get { return Profile.DisplayName; } }
            public string Caption { get { return Profile.Caption; } }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
