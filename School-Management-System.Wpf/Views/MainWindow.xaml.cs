using System;
using System.Windows;
using School_Management_System.DataLayer;
using School_Management_System.Presentation.Forms;
using School_Management_System.Wpf.Services;
using WinForms = System.Windows.Forms;

namespace School_Management_System.Wpf.Views
{
    public partial class MainWindow : Window
    {
        private readonly DashboardForm _dashboardForm;
        private bool _isClosingFromLogout;

        public MainWindow(DatabaseHelper database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            InitializeComponent();
            Icon = BrandingAssetLoader.LoadAppIcon();

            _dashboardForm = new DashboardForm(database);
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        public bool LogoutRequested { get; private set; }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DashboardHost.Child != null)
            {
                return;
            }

            _dashboardForm.TopLevel = false;
            _dashboardForm.FormBorderStyle = WinForms.FormBorderStyle.None;
            _dashboardForm.ShowInTaskbar = false;
            _dashboardForm.WindowState = WinForms.FormWindowState.Normal;
            _dashboardForm.Dock = WinForms.DockStyle.Fill;
            _dashboardForm.LogoutRequested += DashboardForm_LogoutRequested;
            _dashboardForm.FormClosed += DashboardForm_FormClosed;

            DashboardHost.Child = _dashboardForm;
            _dashboardForm.Show();
        }

        private void DashboardForm_LogoutRequested(object sender, EventArgs e)
        {
            LogoutRequested = true;
            _isClosingFromLogout = true;
            Close();
        }

        private void DashboardForm_FormClosed(object sender, WinForms.FormClosedEventArgs e)
        {
            if (IsLoaded && !_isClosingFromLogout)
            {
                Close();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _dashboardForm.LogoutRequested -= DashboardForm_LogoutRequested;
            _dashboardForm.FormClosed -= DashboardForm_FormClosed;

            if (DashboardHost.Child == _dashboardForm)
            {
                DashboardHost.Child = null;
            }

            if (!_dashboardForm.IsDisposed)
            {
                _dashboardForm.Dispose();
            }
        }
    }
}
