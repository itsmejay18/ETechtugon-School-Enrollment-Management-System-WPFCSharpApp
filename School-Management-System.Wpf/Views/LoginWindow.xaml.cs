using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels;

namespace School_Management_System.Wpf.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += LoginWindow_Loaded;
            ApplyBrandingAssets();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyQuickLoginPreset();
            UsernameInput.Focus();
            UsernameInput.SelectAll();
        }

        private void ApplyBrandingAssets()
        {
            var brandLogo = BrandingAssetLoader.LoadBrandLogo();
            var appIcon = BrandingAssetLoader.LoadAppIcon();

            if (appIcon != null)
            {
                Icon = appIcon;
            }

            if (brandLogo != null)
            {
                BrandLogoImage.Source = brandLogo;
            }
        }

        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
                e.Handled = true;
            }
        }

        private void QuickLoginCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyQuickLoginPreset();
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (VisiblePasswordInput.Visibility == Visibility.Visible &&
                VisiblePasswordInput.Text != PasswordInput.Password)
            {
                VisiblePasswordInput.Text = PasswordInput.Password;
            }
        }

        private void VisiblePasswordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VisiblePasswordInput.Visibility == Visibility.Visible &&
                PasswordInput.Password != VisiblePasswordInput.Text)
            {
                PasswordInput.Password = VisiblePasswordInput.Text;
            }
        }

        private void ShowPasswordToggle_Changed(object sender, RoutedEventArgs e)
        {
            var showPassword = ShowPasswordToggle.IsChecked == true;
            VisiblePasswordInput.Visibility = showPassword ? Visibility.Visible : Visibility.Collapsed;
            PasswordInput.Visibility = showPassword ? Visibility.Collapsed : Visibility.Visible;

            if (showPassword)
            {
                VisiblePasswordInput.Text = PasswordInput.Password;
                VisiblePasswordInput.Focus();
                VisiblePasswordInput.SelectAll();
            }
            else
            {
                PasswordInput.Password = VisiblePasswordInput.Text;
                PasswordInput.Focus();
                PasswordInput.SelectAll();
            }
        }

        private void TryLogin()
        {
            var vm = DataContext as LoginViewModel;
            if (vm == null)
            {
                return;
            }

            if (vm.SelectedQuickLogin != null &&
                !string.IsNullOrWhiteSpace(vm.SelectedQuickLogin.Username) &&
                string.IsNullOrWhiteSpace(GetPasswordText()))
            {
                SetPasswordText(vm.SelectedQuickLogin.Password);
            }

            vm.TryLogin(GetPasswordText());
        }

        private void ApplyQuickLoginPreset()
        {
            var selected = QuickLoginCombo.SelectedItem as LoginViewModel.QuickLoginOption;
            if (selected == null || string.IsNullOrWhiteSpace(selected.Username))
            {
                return;
            }

            var vm = DataContext as LoginViewModel;
            if (vm != null)
            {
                vm.Username = selected.Username;
            }

            SetPasswordText(selected.Password ?? string.Empty);

            if (ShowPasswordToggle.IsChecked == true)
            {
                VisiblePasswordInput.Focus();
                VisiblePasswordInput.SelectAll();
            }
            else
            {
                PasswordInput.Focus();
                PasswordInput.SelectAll();
            }
        }

        private string GetPasswordText()
        {
            return ShowPasswordToggle.IsChecked == true
                ? VisiblePasswordInput.Text ?? string.Empty
                : PasswordInput.Password ?? string.Empty;
        }

        private void SetPasswordText(string password)
        {
            var safePassword = password ?? string.Empty;

            if (PasswordInput.Password != safePassword)
            {
                PasswordInput.Password = safePassword;
            }

            if (VisiblePasswordInput.Text != safePassword)
            {
                VisiblePasswordInput.Text = safePassword;
            }
        }
    }
}
