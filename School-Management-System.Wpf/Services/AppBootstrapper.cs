using System;
using System.Configuration;
using School_Management_System.BusinessLayer.Session;
using School_Management_System.BusinessLayer.Services;
using School_Management_System.DataLayer;
using School_Management_System.DataLayer.Configuration;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.Wpf.Services
{
    public sealed class AppBootstrapper
    {
        private AppBootstrapper(
            DatabaseHelper database,
            CourseService courseService,
            DepartmentService departmentService,
            AuthService authService,
            ActivityLogService activityLogService)
        {
            Database = database;
            CourseService = courseService;
            DepartmentService = departmentService;
            AuthService = authService;
            ActivityLogService = activityLogService;
        }

        public DatabaseHelper Database { get; private set; }
        public CourseService CourseService { get; private set; }
        public DepartmentService DepartmentService { get; private set; }
        public AuthService AuthService { get; private set; }
        public ActivityLogService ActivityLogService { get; private set; }

        public static void InitializeRuntimeConnectionMode(string[] args)
        {
            ApplyConnectionMode(args);
        }

        public static AppBootstrapper Create(string[] args)
        {
            ApplyConnectionMode(args);
            return CreateForCurrentMode();
        }

        public static AppBootstrapper CreateForCurrentMode()
        {
            return BuildForCurrentMode();
        }

        private static AppBootstrapper BuildForCurrentMode()
        {
            var database = DatabaseHelper.FromConfig();

            string error;
            if (!database.TestConnection(out error))
            {
                throw new InvalidOperationException(BuildConnectionMessage(error));
            }

            SchemaMigrationRunner.EnsureCurrent(database);

            IDepartmentData departmentData = new DepartmentData(database);
            ICourseData courseData = new CourseData(database);
            IUserData userData = new UserData(database);
            IActivityLogData activityLogData = new ActivityLogData(database);
            var activityLogService = new ActivityLogService(activityLogData);

            return new AppBootstrapper(
                database,
                new CourseService(courseData),
                new DepartmentService(departmentData),
                new AuthService(userData, activityLogService),
                activityLogService);
        }

        private static void ApplyConnectionMode(string[] args)
        {
            var defaultMode = ConnectionModeHelper.GetCurrentMode("Online");

            ConnectionModeHelper.ApplyRuntimeMode(defaultMode);

            if (args == null)
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
                if (string.Equals(token, "--network", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, "--wired", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionModeHelper.ApplyRuntimeMode("Wired");
                    continue;
                }

                if (string.Equals(token, "--wireless", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionModeHelper.ApplyRuntimeMode("Wireless");
                    continue;
                }

                if (string.Equals(token, "--local", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionModeHelper.ApplyRuntimeMode("Local");
                    continue;
                }

                if (string.Equals(token, "--online", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token, "--hostinger", StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionModeHelper.ApplyRuntimeMode("Online");
                    continue;
                }

                const string longPrefix = "--db-mode=";
                if (token.StartsWith(longPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    ConnectionModeHelper.ApplyRuntimeMode(token.Substring(longPrefix.Length));
                }
            }
        }

        private static string BuildConnectionMessage(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return "Unable to connect to the database. Check the configured profile and try again.";
            }

            return "Unable to start the WPF migration shell.\n\n" + error.Trim();
        }
    }
}
