using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.Common;
using School_Management_System.Models;
using School_Management_System.Wpf.Infrastructure;
using School_Management_System.Wpf.Services;

namespace School_Management_System.Wpf.ViewModels.Settings
{
    public sealed class CompanyBrandingViewModel : ViewModelBase
    {
        private readonly BrandingProfileService _brandingProfileService;
        private readonly ActivityLogService _activityLogService;
        private string _companyName;
        private string _applicationTagline;
        private string _shellWorkspaceTagline;
        private string _dashboardTitle;
        private string _dashboardSubtitle;
        private string _loginHeadline;
        private string _loginBody;
        private string _loginFormTitle;
        private string _loginFormSubtitle;
        private string _featureOneTitle;
        private string _featureOneBody;
        private string _featureTwoTitle;
        private string _featureTwoBody;
        private string _featureThreeTitle;
        private string _featureThreeBody;
        private string _clientSerialNumber;
        private string _supportEmail;
        private string _supportPhoneNumber;
        private string _companyAddress;
        private string _statusMessage;
        private byte[] _brandLogoBytes;
        private byte[] _compactLogoBytes;
        private ImageSource _brandLogo;
        private ImageSource _compactLogo;

        public CompanyBrandingViewModel(BrandingProfileService brandingProfileService, ActivityLogService activityLogService)
        {
            _brandingProfileService = brandingProfileService ?? throw new ArgumentNullException(nameof(brandingProfileService));
            _activityLogService = activityLogService;

            UploadBrandLogoCommand = new RelayCommand(UploadBrandLogo);
            RemoveBrandLogoCommand = new RelayCommand(RemoveBrandLogo, () => HasBrandLogo);
            UploadCompactLogoCommand = new RelayCommand(UploadCompactLogo);
            RemoveCompactLogoCommand = new RelayCommand(RemoveCompactLogo, () => HasCompactLogo);
            SaveCommand = new RelayCommand(Save);

            Load();
        }

        public RelayCommand UploadBrandLogoCommand { get; private set; }
        public RelayCommand RemoveBrandLogoCommand { get; private set; }
        public RelayCommand UploadCompactLogoCommand { get; private set; }
        public RelayCommand RemoveCompactLogoCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        public string CompanyName
        {
            get { return _companyName; }
            set { SetProperty(ref _companyName, value); }
        }

        public string ApplicationTagline
        {
            get { return _applicationTagline; }
            set { SetProperty(ref _applicationTagline, value); }
        }

        public string ShellWorkspaceTagline
        {
            get { return _shellWorkspaceTagline; }
            set { SetProperty(ref _shellWorkspaceTagline, value); }
        }

        public string LoginHeadline
        {
            get { return _loginHeadline; }
            set { SetProperty(ref _loginHeadline, value); }
        }

        public string LoginBody
        {
            get { return _loginBody; }
            set { SetProperty(ref _loginBody, value); }
        }

        public string LoginFormTitle
        {
            get { return _loginFormTitle; }
            set { SetProperty(ref _loginFormTitle, value); }
        }

        public string LoginFormSubtitle
        {
            get { return _loginFormSubtitle; }
            set { SetProperty(ref _loginFormSubtitle, value); }
        }

        public string FeatureOneTitle
        {
            get { return _featureOneTitle; }
            set { SetProperty(ref _featureOneTitle, value); }
        }

        public string FeatureOneBody
        {
            get { return _featureOneBody; }
            set { SetProperty(ref _featureOneBody, value); }
        }

        public string FeatureTwoTitle
        {
            get { return _featureTwoTitle; }
            set { SetProperty(ref _featureTwoTitle, value); }
        }

        public string FeatureTwoBody
        {
            get { return _featureTwoBody; }
            set { SetProperty(ref _featureTwoBody, value); }
        }

        public string FeatureThreeTitle
        {
            get { return _featureThreeTitle; }
            set { SetProperty(ref _featureThreeTitle, value); }
        }

        public string FeatureThreeBody
        {
            get { return _featureThreeBody; }
            set { SetProperty(ref _featureThreeBody, value); }
        }

        public string ClientSerialNumber
        {
            get { return _clientSerialNumber; }
            set { SetProperty(ref _clientSerialNumber, value); }
        }

        public string SupportEmail
        {
            get { return _supportEmail; }
            set { SetProperty(ref _supportEmail, value); }
        }

        public string SupportPhoneNumber
        {
            get { return _supportPhoneNumber; }
            set { SetProperty(ref _supportPhoneNumber, value); }
        }

        public string CompanyAddress
        {
            get { return _companyAddress; }
            set { SetProperty(ref _companyAddress, value); }
        }

        public ImageSource BrandLogo
        {
            get { return _brandLogo; }
            private set
            {
                if (SetProperty(ref _brandLogo, value))
                {
                    OnPropertyChanged(nameof(HasBrandLogo));
                    RemoveBrandLogoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ImageSource CompactLogo
        {
            get { return _compactLogo; }
            private set
            {
                if (SetProperty(ref _compactLogo, value))
                {
                    OnPropertyChanged(nameof(HasCompactLogo));
                    RemoveCompactLogoCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasBrandLogo
        {
            get { return BrandLogo != null; }
        }

        public bool HasCompactLogo
        {
            get { return CompactLogo != null; }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        private void Load()
        {
            var profile = _brandingProfileService.GetCurrent();

            CompanyName = profile.CompanyName;
            ApplicationTagline = profile.ApplicationTagline;
            ShellWorkspaceTagline = profile.ShellWorkspaceTagline;
            _dashboardTitle = profile.DashboardTitle;
            _dashboardSubtitle = profile.DashboardSubtitle;
            LoginHeadline = profile.LoginHeadline;
            LoginBody = profile.LoginBody;
            LoginFormTitle = profile.LoginFormTitle;
            LoginFormSubtitle = profile.LoginFormSubtitle;
            FeatureOneTitle = profile.FeatureOneTitle;
            FeatureOneBody = profile.FeatureOneBody;
            FeatureTwoTitle = profile.FeatureTwoTitle;
            FeatureTwoBody = profile.FeatureTwoBody;
            FeatureThreeTitle = profile.FeatureThreeTitle;
            FeatureThreeBody = profile.FeatureThreeBody;
            ClientSerialNumber = profile.ClientSerialNumber;
            SupportEmail = profile.SupportEmail;
            SupportPhoneNumber = profile.SupportPhoneNumber;
            CompanyAddress = profile.CompanyAddress;

            _brandLogoBytes = profile.BrandLogoData;
            _compactLogoBytes = profile.CompactLogoData;
            BrandLogo = WpfUiDataHelper.LoadImage(_brandLogoBytes);
            CompactLogo = WpfUiDataHelper.LoadImage(_compactLogoBytes);
            StatusMessage = "Company branding is loaded from the active database profile.";
        }

        private void UploadBrandLogo()
        {
            var bytes = ChooseImageBytes("Select the header branding logo");
            if (bytes == null)
            {
                return;
            }

            _brandLogoBytes = bytes;
            BrandLogo = WpfUiDataHelper.LoadImage(bytes);
            StatusMessage = "Header branding logo loaded. Save settings to apply it.";
        }

        private void RemoveBrandLogo()
        {
            _brandLogoBytes = null;
            BrandLogo = null;
            StatusMessage = "Header branding logo removed. Save settings to apply the change.";
        }

        private void UploadCompactLogo()
        {
            var bytes = ChooseImageBytes("Select the dashboard company logo");
            if (bytes == null)
            {
                return;
            }

            _compactLogoBytes = bytes;
            CompactLogo = WpfUiDataHelper.LoadImage(bytes);
            StatusMessage = "Dashboard company logo loaded. Save settings to apply it.";
        }

        private void RemoveCompactLogo()
        {
            _compactLogoBytes = null;
            CompactLogo = null;
            StatusMessage = "Dashboard company logo removed. Save settings to apply the change.";
        }

        private void Save()
        {
            try
            {
                var saved = _brandingProfileService.Save(new BrandingProfile
                {
                    BrandingProfileId = 1,
                    CompanyName = CompanyName,
                    ApplicationTagline = ApplicationTagline,
                    ShellWorkspaceTagline = ShellWorkspaceTagline,
                    DashboardTitle = _dashboardTitle,
                    DashboardSubtitle = _dashboardSubtitle,
                    LoginHeadline = LoginHeadline,
                    LoginBody = LoginBody,
                    LoginFormTitle = LoginFormTitle,
                    LoginFormSubtitle = LoginFormSubtitle,
                    FeatureOneTitle = FeatureOneTitle,
                    FeatureOneBody = FeatureOneBody,
                    FeatureTwoTitle = FeatureTwoTitle,
                    FeatureTwoBody = FeatureTwoBody,
                    FeatureThreeTitle = FeatureThreeTitle,
                    FeatureThreeBody = FeatureThreeBody,
                    ClientSerialNumber = ClientSerialNumber,
                    SupportEmail = SupportEmail,
                    SupportPhoneNumber = SupportPhoneNumber,
                    CompanyAddress = CompanyAddress,
                    BrandLogoData = _brandLogoBytes,
                    CompactLogoData = _compactLogoBytes
                });

                SchoolBranding.Apply(saved);
                LogBrandingTransaction(saved);
                StatusMessage = "Branding saved to the active database profile.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Company Branding",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static byte[] ChooseImageBytes(string title)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Title = title
            };

            if (dialog.ShowDialog() != true)
            {
                return null;
            }

            byte[] bytes;
            string errorMessage;
            if (ImageFileReader.TryReadImageBytes(dialog.FileName, out bytes, out errorMessage))
            {
                return bytes;
            }

            MessageBox.Show(
                errorMessage,
                "Company Branding",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return null;
        }

        private void LogBrandingTransaction(BrandingProfile profile)
        {
            if (_activityLogService == null)
            {
                return;
            }

            _activityLogService.LogTransaction(
                AppConstants.ActivityActions.Update,
                AppConstants.Entities.BrandingProfile,
                profile == null ? (int?)null : profile.BrandingProfileId,
                "Updated company profile for " + (profile == null || string.IsNullOrWhiteSpace(profile.CompanyName) ? "the active school" : profile.CompanyName.Trim()) + ".");
        }
    }
}
