using System;
using System.Windows.Controls;
using System.Windows.Threading;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.ViewModels;

namespace School_Management_System.Wpf.Views
{
    public partial class DashboardHomeView : UserControl
    {
        private readonly DispatcherTimer _clockTimer;

        public DashboardHomeView()
        {
            InitializeComponent();

            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += ClockTimer_Tick;

            Loaded += DashboardHomeView_Loaded;
            Unloaded += DashboardHomeView_Unloaded;
        }

        private void DashboardHomeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            SchoolBranding.BrandingChanged += SchoolBranding_BrandingChanged;
            ApplyBrandingAssets();
            RefreshDashboard();
            _clockTimer.Start();
        }

        private void DashboardHomeView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _clockTimer.Stop();
            SchoolBranding.BrandingChanged -= SchoolBranding_BrandingChanged;
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            var viewModel = DataContext as DashboardHomeViewModel;
            if (viewModel != null)
            {
                viewModel.RefreshClock();
            }
        }

        private void RefreshDashboard()
        {
            var viewModel = DataContext as DashboardHomeViewModel;
            if (viewModel != null)
            {
                viewModel.RefreshMetrics();
                viewModel.RefreshClock();
            }
        }

        private void ApplyBrandingAssets()
        {
            HeroBrandLogoImage.Source = BrandingAssetLoader.LoadCompanyLogo();
        }

        private void SchoolBranding_BrandingChanged(object sender, EventArgs e)
        {
            ApplyBrandingAssets();
        }
    }
}
