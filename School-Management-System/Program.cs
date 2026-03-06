using System;
using System.Windows.Forms;
using School_Management_System.DataLayer.Logging;
using School_Management_System.Presentation.Forms;

namespace School_Management_System
{
    internal static class Program
    {
        private static bool _isHandlingFatal;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplyRuntimeConnectionMode(args);
            WireGlobalExceptionHandlers();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }

        private static void ApplyRuntimeConnectionMode(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            for (var i = 0; i < args.Length; i++)
            {
                var token = args[i] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                token = token.Trim();

                if (string.Equals(token, "--network", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("SMS_DB_MODE", "Wired", EnvironmentVariableTarget.Process);
                    continue;
                }

                if (string.Equals(token, "--wired", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("SMS_DB_MODE", "Wired", EnvironmentVariableTarget.Process);
                    continue;
                }

                if (string.Equals(token, "--wireless", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("SMS_DB_MODE", "Wireless", EnvironmentVariableTarget.Process);
                    continue;
                }

                if (string.Equals(token, "--local", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("SMS_DB_MODE", "Local", EnvironmentVariableTarget.Process);
                    continue;
                }

                if (string.Equals(token, "--online", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, "--hostinger", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("SMS_DB_MODE", "Online", EnvironmentVariableTarget.Process);
                    continue;
                }

                const string longPrefix = "--db-mode=";
                if (token.StartsWith(longPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var mode = token.Substring(longPrefix.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(mode))
                    {
                        Environment.SetEnvironmentVariable("SMS_DB_MODE", mode, EnvironmentVariableTarget.Process);
                    }
                    continue;
                }

                const string slashPrefix = "/dbmode:";
                if (token.StartsWith(slashPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var mode = token.Substring(slashPrefix.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(mode))
                    {
                        Environment.SetEnvironmentVariable("SMS_DB_MODE", mode, EnvironmentVariableTarget.Process);
                    }
                }
            }
        }

        private static void WireGlobalExceptionHandlers()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => HandleUnhandledException("UI", e == null ? null : e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                HandleUnhandledException("AppDomain", e == null ? null : e.ExceptionObject as Exception);
        }

        private static void HandleUnhandledException(string source, Exception ex)
        {
            if (_isHandlingFatal)
            {
                return;
            }

            _isHandlingFatal = true;
            try
            {
                FileLogger.LogError("Program.Unhandled." + (source ?? "Unknown"), ex ?? new Exception("Unhandled exception without details."));
                var detail = ex == null || string.IsNullOrWhiteSpace(ex.Message)
                    ? "Unexpected application error."
                    : ex.Message.Trim();
                if (detail.Length > 200)
                {
                    detail = detail.Substring(0, 200) + "...";
                }

                ThemedMessageBox.ShowError(
                    null,
                    "An unexpected error occurred.\n" + detail + "\n\nCheck logs in the application folder for details.",
                    "Application Error");
            }
            finally
            {
                _isHandlingFatal = false;
            }
        }
    }
}
