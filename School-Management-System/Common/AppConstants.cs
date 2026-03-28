using System;

namespace School_Management_System.Common
{
    public static class AppConstants
    {
        public const string AppTitle = "School Enrollment Management System";
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
            public const string Database = "Database";
            public const string SystemSetting = "SystemSetting";
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

            public const string DbHostOnline = "DbHost.Online";
            public const string DbPortOnline = "DbPort.Online";
            public const string DbNameOnline = "DbName.Online";
            public const string DbUserOnline = "DbUser.Online";
            public const string DbPasswordOnline = "DbPassword.Online";
            public const string DbSslModeOnline = "DbSslMode.Online";
            public const string DbSslCaPathOnline = "DbSslCaPath.Online";

            // Legacy key names kept for backward compatibility.
            public const string DbHostNetwork = "DbHost.Network";
            public const string DbPortNetwork = "DbPort.Network";
            public const string DbNameNetwork = "DbName.Network";

            public const string BackupDirectory = "Backup.Directory";
            public const string BackupPreferredType = "Backup.PreferredType";

            public const string TuitionPerUnit = "Billing.TuitionPerUnit";
            public const string MiscellaneousFee = "Billing.MiscellaneousFee";
            public const string RegistrationFee = "Billing.RegistrationFee";
            public const string LaboratoryFee = "Billing.LaboratoryFee";
        }

        public static class ActivityActions
        {
            public const string LoginSuccess = "LoginSuccess";
            public const string LoginFailed = "LoginFailed";
            public const string Logout = "Logout";
            public const string BackupFull = "BackupFull";
            public const string BackupIncremental = "BackupIncremental";
            public const string BackupDifferential = "BackupDifferential";
            public const string RestoreDatabase = "RestoreDatabase";
            public const string BackupError = "BackupError";
            public const string RestoreError = "RestoreError";
        }
    }
}

