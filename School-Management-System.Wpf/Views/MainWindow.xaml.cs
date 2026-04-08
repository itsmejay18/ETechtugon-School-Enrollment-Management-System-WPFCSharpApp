using System;
using System.Windows;
using System.Windows.Input;
using School_Management_System.Models;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels;

namespace School_Management_System.Wpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(AppBootstrapper bootstrapper, User currentUser)
        {
            if (bootstrapper == null) throw new ArgumentNullException(nameof(bootstrapper));

            InitializeComponent();

            Icon = BrandingAssetLoader.LoadAppIcon();
            DataContext = new ShellViewModel(bootstrapper, currentUser, RequestLogout);

            ApplyBrandingAssets();
            SourceInitialized += MainWindow_SourceInitialized;
            Loaded += MainWindow_Loaded;
            Activated += MainWindow_Activated;
            StateChanged += MainWindow_StateChanged;
            UpdateWindowStateGlyph();
        }

        public bool LogoutRequested { get; private set; }

        private void ApplyBrandingAssets()
        {
            var logo = BrandingAssetLoader.LoadBrandLogo();
            if (logo != null)
            {
                ShellBrandLogoImage.Source = logo;
            }
        }

        private void RequestLogout()
        {
            LogoutRequested = true;
            Close();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            EnterFullscreen();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnterFullscreen();
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (Topmost && WindowState != WindowState.Minimized)
            {
                EnterFullscreen();
            }

            UpdateWindowStateGlyph();
        }

        private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.ClickCount == 2)
            {
                ToggleWindowState();
                return;
            }

            if (Topmost)
            {
                return;
            }

            try
            {
                DragMove();
            }
            catch
            {
            }
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleWindowState()
        {
            if (Topmost)
            {
                Topmost = false;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
                Width = 1500;
                Height = 920;
                Left = SystemParameters.WorkArea.Left + ((SystemParameters.WorkArea.Width - Width) / 2d);
                Top = SystemParameters.WorkArea.Top + ((SystemParameters.WorkArea.Height - Height) / 2d);
            }
            else
            {
                EnterFullscreen();
            }

            UpdateWindowStateGlyph();
        }

        private void EnterFullscreen()
        {
            ResizeMode = ResizeMode.NoResize;
            WindowFullscreenHelper.ApplyMonitorBounds(this, true);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateWindowStateGlyph();
        }

        private void UpdateWindowStateGlyph()
        {
            if (MaximizeRestoreIcon != null)
            {
                MaximizeRestoreIcon.Text = Topmost
                    ? "\uE923"
                    : "\uE922";
            }
        }
    }
}
