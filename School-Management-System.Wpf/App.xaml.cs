using System;
using System.Windows;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.Wpf.Services;
using School_Management_System.Wpf.Views;
using School_Management_System.Wpf.ViewModels;

namespace School_Management_System.Wpf
{
    public partial class App : Application
    {
        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown;
                AppBootstrapper.InitializeRuntimeConnectionMode(e.Args);
                RunLoginShellLoop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "WPF Migration Startup",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
            }
        }

        private void RunLoginShellLoop()
        {
            while (true)
            {
                var loginViewModel = new LoginViewModel(new LoginConnectionService());
                var loginWindow = new LoginWindow
                {
                    DataContext = loginViewModel
                };

                loginViewModel.CloseRequested += result =>
                {
                    loginWindow.DialogResult = result;
                    loginWindow.Close();
                };

                var loginResult = loginWindow.ShowDialog();
                if (loginResult != true || loginViewModel.AuthenticatedUser == null)
                {
                    Shutdown(0);
                    return;
                }

                var bootstrapper = AppBootstrapper.CreateForCurrentMode();
                UserSession.Start(loginViewModel.AuthenticatedUser);

                var window = new MainWindow(bootstrapper, loginViewModel.AuthenticatedUser);
                MainWindow = window;

                try
                {
                    window.ShowDialog();
                }
                finally
                {
                    UserSession.End();
                    MainWindow = null;
                }

                if (!window.LogoutRequested)
                {
                    Shutdown(0);
                    return;
                }
            }
        }
    }
}
