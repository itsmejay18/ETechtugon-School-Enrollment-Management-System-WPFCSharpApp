using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.DataLayer.Logging;
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
                WireGlobalExceptionHandlers();
                AppBootstrapper.InitializeRuntimeConnectionMode(e.Args);
                RunLoginShellLoop();
            }
            catch (Exception ex)
            {
                FileLogger.LogError("App.OnStartup", ex);
                MessageBox.Show(
                    ex.Message,
                    "School Management System",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
            }
        }

        private void WireGlobalExceptionHandlers()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            FileLogger.LogError("App.DispatcherUnhandledException", e.Exception);
            try
            {
                MessageBox.Show(
                    "An unexpected error occurred and was logged.\n\n" + e.Exception.Message,
                    "School Management System",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // Best effort. Avoid re-entering the dispatcher loop.
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                FileLogger.LogError("AppDomain.UnhandledException (terminating=" + e.IsTerminating + ")", ex);
            }
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            FileLogger.LogError("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
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
