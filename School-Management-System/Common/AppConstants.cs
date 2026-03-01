using System;

namespace School_Management_System.Common
{
    public static class AppConstants
    {
        public const string AppTitle = "School Management System";
        public const string ConnectionStringName = "SchoolDb";

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Registrar = "Registrar";
            public const string Faculty = "Faculty";
        }

        public static class Entities
        {
            public const string User = "User";
            public const string Student = "Student";
            public const string Faculty = "Faculty";
            public const string Course = "Course";
            public const string Subject = "Subject";
            public const string Curriculum = "Curriculum";
            public const string Enrollment = "Enrollment";
        }

        public static class SettingKeys
        {
            public const string CurrentAcademicYearId = "CurrentAcademicYearId";
            public const string CurrentSemesterId = "CurrentSemesterId";

            public const string DbConnectionMode = "DbConnectionMode";

            public const string DbHostLocal = "DbHost.Local";
            public const string DbPortLocal = "DbPort.Local";
            public const string DbNameLocal = "DbName.Local";
            public const string DbUserLocal = "DbUser.Local";
            public const string DbPasswordLocal = "DbPassword.Local";

            public const string DbHostWired = "DbHost.Wired";
            public const string DbPortWired = "DbPort.Wired";
            public const string DbNameWired = "DbName.Wired";
            public const string DbUserWired = "DbUser.Wired";
            public const string DbPasswordWired = "DbPassword.Wired";

            public const string DbHostWireless = "DbHost.Wireless";
            public const string DbPortWireless = "DbPort.Wireless";
            public const string DbNameWireless = "DbName.Wireless";
            public const string DbUserWireless = "DbUser.Wireless";
            public const string DbPasswordWireless = "DbPassword.Wireless";

            // Legacy key names kept for backward compatibility.
            public const string DbHostNetwork = "DbHost.Network";
            public const string DbPortNetwork = "DbPort.Network";
            public const string DbNameNetwork = "DbName.Network";
        }
    }
}

