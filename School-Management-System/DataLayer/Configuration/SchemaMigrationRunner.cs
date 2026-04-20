using System;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;

namespace School_Management_System.DataLayer.Configuration
{
    public static class SchemaMigrationRunner
    {
        private static readonly object Sync = new object();

        public static void EnsureCurrent(DatabaseHelper db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            lock (Sync)
            {
                EnsureMigrationTable(db);
                ApplyMigration2026030701(db);
                ApplyMigration2026030702(db);
                ApplyMigration2026030703(db);
                ApplyMigration2026031501(db);
                ApplyMigration2026041701(db);
            }
        }

        private static void EnsureMigrationTable(DatabaseHelper db)
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS `schemamigration` (
  `MigrationId` varchar(50) NOT NULL,
  `Description` varchar(255) NOT NULL,
  `AppliedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            db.ExecuteNonQuery(sql, CommandType.Text, null);
        }

        private static bool IsApplied(DatabaseHelper db, string migrationId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM `schemamigration`
WHERE `MigrationId` = @MigrationId;";

            var count = Convert.ToInt32(
                db.ExecuteScalar(
                    sql,
                    CommandType.Text,
                    new[] { new MySqlParameter("@MigrationId", migrationId) }));

            return count > 0;
        }

        private static void MarkApplied(DatabaseHelper db, string migrationId, string description)
        {
            const string sql = @"
INSERT INTO `schemamigration` (`MigrationId`, `Description`, `AppliedAt`)
VALUES (@MigrationId, @Description, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
  `Description` = VALUES(`Description`),
  `AppliedAt` = `AppliedAt`;";

            db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@MigrationId", migrationId),
                    new MySqlParameter("@Description", description ?? string.Empty)
                });
        }

        private static void ApplyMigration2026030701(DatabaseHelper db)
        {
            const string migrationId = "2026030701";
            if (IsApplied(db, migrationId))
            {
                return;
            }

            EnsureColumnExists(db, "users", "PhotoPath", "ALTER TABLE `users` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `DisplayName`;");
            EnsureColumnExists(db, "faculty", "PhotoPath", "ALTER TABLE `faculty` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`;");
            EnsureColumnExists(db, "student", "PhotoPath", "ALTER TABLE `student` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`;");

            MarkApplied(db, migrationId, "Ensure photo-path columns in users/faculty/student.");
        }

        private static void ApplyMigration2026030702(DatabaseHelper db)
        {
            const string migrationId = "2026030702";
            if (IsApplied(db, migrationId))
            {
                return;
            }

            EnsureActivityLogTable(db);
            EnsureColumnExists(db, "activitylog", "MachineName", "ALTER TABLE `activitylog` ADD COLUMN `MachineName` varchar(100) DEFAULT NULL AFTER `Details`;");

            MarkApplied(db, migrationId, "Ensure activitylog table and machinename column.");
        }

        private static void ApplyMigration2026030703(DatabaseHelper db)
        {
            const string migrationId = "2026030703";
            if (IsApplied(db, migrationId))
            {
                return;
            }

            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbHostOnline, "localhost");
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbPortOnline, "3306");
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbNameOnline, "schoolmanagementsystem");
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbUserOnline, string.Empty);
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbPasswordOnline, string.Empty);
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbSslModeOnline, "Required");
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.DbSslCaPathOnline, string.Empty);
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.BackupDirectory, string.Empty);
            EnsureSystemSettingKey(db, AppConstants.SettingKeys.BackupPreferredType, "Full");

            MarkApplied(db, migrationId, "Ensure online TLS and backup setting keys.");
        }

        private static void ApplyMigration2026031501(DatabaseHelper db)
        {
            const string migrationId = "2026031501";
            if (IsApplied(db, migrationId))
            {
                return;
            }

            EnsureSemesterRow(db, 1, "1st Semester", 1);
            EnsureSemesterRow(db, 2, "2nd Semester", 2);

            const string normalizeCurrentSemesterSql = @"
UPDATE `systemsetting`
SET `SettingValue` = CASE
    WHEN CAST(`SettingValue` AS UNSIGNED) = 2 THEN '2'
    WHEN CAST(`SettingValue` AS UNSIGNED) > 2 AND MOD(CAST(`SettingValue` AS UNSIGNED), 2) = 0 THEN '2'
    ELSE '1'
END
WHERE `SettingKey` = @SettingKey;";

            db.ExecuteNonQuery(
                normalizeCurrentSemesterSql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SettingKey", AppConstants.SettingKeys.CurrentSemesterId)
                });

            const string normalizeSemesterRowsSql = @"
UPDATE `semester`
SET
  `Name` = CASE
      WHEN `SemesterId` = 1 THEN '1st Semester'
      WHEN `SemesterId` = 2 THEN '2nd Semester'
      ELSE `Name`
  END,
  `SortOrder` = CASE
      WHEN `SemesterId` = 1 THEN 1
      WHEN `SemesterId` = 2 THEN 2
      ELSE `SortOrder`
  END,
  `IsActive` = CASE
      WHEN `SemesterId` IN (1, 2) THEN 1
      ELSE 0
  END;";

            db.ExecuteNonQuery(normalizeSemesterRowsSql, CommandType.Text, null);

            MarkApplied(db, migrationId, "Normalize semester catalog to 1st and 2nd semester only.");
        }

        private static void ApplyMigration2026041701(DatabaseHelper db)
        {
            const string migrationId = "2026041701";
            if (IsApplied(db, migrationId))
            {
                return;
            }

            EnsureBrandingProfileTable(db);

            const string seedSql = @"
INSERT INTO `brandingprofile`
(
  `BrandingProfileId`,
  `CompanyName`,
  `ApplicationTagline`,
  `ShellWorkspaceTagline`,
  `DashboardTitle`,
  `DashboardSubtitle`,
  `LoginHeadline`,
  `LoginBody`,
  `LoginFormTitle`,
  `LoginFormSubtitle`,
  `FeatureOneTitle`,
  `FeatureOneBody`,
  `FeatureTwoTitle`,
  `FeatureTwoBody`,
  `FeatureThreeTitle`,
  `FeatureThreeBody`,
  `ClientSerialNumber`,
  `SupportEmail`,
  `SupportPhoneNumber`,
  `CompanyAddress`,
  `UpdatedAt`
)
SELECT
  @BrandingProfileId,
  @CompanyName,
  @ApplicationTagline,
  @ShellWorkspaceTagline,
  @DashboardTitle,
  @DashboardSubtitle,
  @LoginHeadline,
  @LoginBody,
  @LoginFormTitle,
  @LoginFormSubtitle,
  @FeatureOneTitle,
  @FeatureOneBody,
  @FeatureTwoTitle,
  @FeatureTwoBody,
  @FeatureThreeTitle,
  @FeatureThreeBody,
  @ClientSerialNumber,
  @SupportEmail,
  @SupportPhoneNumber,
  @CompanyAddress,
  UTC_TIMESTAMP()
WHERE NOT EXISTS (SELECT 1 FROM `brandingprofile` LIMIT 1);";

            db.ExecuteNonQuery(
                seedSql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@BrandingProfileId", BrandingDefaults.ProfileId),
                    new MySqlParameter("@CompanyName", BrandingDefaults.CompanyName),
                    new MySqlParameter("@ApplicationTagline", BrandingDefaults.ApplicationTagline),
                    new MySqlParameter("@ShellWorkspaceTagline", BrandingDefaults.ShellWorkspaceTagline),
                    new MySqlParameter("@DashboardTitle", BrandingDefaults.DashboardTitle),
                    new MySqlParameter("@DashboardSubtitle", BrandingDefaults.DashboardSubtitle),
                    new MySqlParameter("@LoginHeadline", BrandingDefaults.LoginHeadline),
                    new MySqlParameter("@LoginBody", BrandingDefaults.LoginBody),
                    new MySqlParameter("@LoginFormTitle", BrandingDefaults.LoginFormTitle),
                    new MySqlParameter("@LoginFormSubtitle", BrandingDefaults.LoginFormSubtitle),
                    new MySqlParameter("@FeatureOneTitle", BrandingDefaults.FeatureOneTitle),
                    new MySqlParameter("@FeatureOneBody", BrandingDefaults.FeatureOneBody),
                    new MySqlParameter("@FeatureTwoTitle", BrandingDefaults.FeatureTwoTitle),
                    new MySqlParameter("@FeatureTwoBody", BrandingDefaults.FeatureTwoBody),
                    new MySqlParameter("@FeatureThreeTitle", BrandingDefaults.FeatureThreeTitle),
                    new MySqlParameter("@FeatureThreeBody", BrandingDefaults.FeatureThreeBody),
                    new MySqlParameter("@ClientSerialNumber", BrandingDefaults.ClientSerialNumber),
                    new MySqlParameter("@SupportEmail", BrandingDefaults.SupportEmail),
                    new MySqlParameter("@SupportPhoneNumber", BrandingDefaults.SupportPhoneNumber),
                    new MySqlParameter("@CompanyAddress", BrandingDefaults.CompanyAddress)
                });

            MarkApplied(db, migrationId, "Ensure brandingprofile table and seeded landing-screen branding data.");
        }

        private static void EnsureActivityLogTable(DatabaseHelper db)
        {
            const string existsSql = @"
SELECT COUNT(1)
FROM information_schema.tables
WHERE table_schema = DATABASE()
  AND table_name = 'activitylog';";

            var exists = Convert.ToInt32(db.ExecuteScalar(existsSql, CommandType.Text, null)) > 0;
            if (exists)
            {
                return;
            }

            const string createSql = @"
CREATE TABLE IF NOT EXISTS `activitylog` (
  `ActivityLogId` int NOT NULL AUTO_INCREMENT,
  `UserId` int DEFAULT NULL,
  `Action` varchar(50) NOT NULL,
  `Entity` varchar(50) DEFAULT NULL,
  `EntityId` int DEFAULT NULL,
  `Details` varchar(4000) DEFAULT NULL,
  `MachineName` varchar(100) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`ActivityLogId`),
  KEY `IX_ActivityLog_CreatedAt` (`CreatedAt`),
  KEY `FK_ActivityLog_Users` (`UserId`),
  CONSTRAINT `FK_ActivityLog_Users` FOREIGN KEY (`UserId`) REFERENCES `users` (`UserId`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            db.ExecuteNonQuery(createSql, CommandType.Text, null);
        }

        private static void EnsureBrandingProfileTable(DatabaseHelper db)
        {
            const string sql = @"
CREATE TABLE IF NOT EXISTS `brandingprofile` (
  `BrandingProfileId` int NOT NULL,
  `CompanyName` varchar(160) NOT NULL,
  `ApplicationTagline` varchar(255) DEFAULT NULL,
  `ShellWorkspaceTagline` varchar(255) DEFAULT NULL,
  `DashboardTitle` varchar(255) DEFAULT NULL,
  `DashboardSubtitle` varchar(255) DEFAULT NULL,
  `LoginHeadline` varchar(255) DEFAULT NULL,
  `LoginBody` text DEFAULT NULL,
  `LoginFormTitle` varchar(160) DEFAULT NULL,
  `LoginFormSubtitle` varchar(255) DEFAULT NULL,
  `FeatureOneTitle` varchar(160) DEFAULT NULL,
  `FeatureOneBody` varchar(255) DEFAULT NULL,
  `FeatureTwoTitle` varchar(160) DEFAULT NULL,
  `FeatureTwoBody` varchar(255) DEFAULT NULL,
  `FeatureThreeTitle` varchar(160) DEFAULT NULL,
  `FeatureThreeBody` varchar(255) DEFAULT NULL,
  `ClientSerialNumber` varchar(120) DEFAULT NULL,
  `SupportEmail` varchar(160) DEFAULT NULL,
  `SupportPhoneNumber` varchar(80) DEFAULT NULL,
  `CompanyAddress` varchar(255) DEFAULT NULL,
  `BrandLogoData` longblob,
  `CompactLogoData` longblob,
  `UpdatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`BrandingProfileId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

            db.ExecuteNonQuery(sql, CommandType.Text, null);
        }

        private static void EnsureColumnExists(DatabaseHelper db, string tableName, string columnName, string alterSql)
        {
            const string existsSql = @"
SELECT COUNT(1)
FROM information_schema.columns
WHERE table_schema = DATABASE()
  AND table_name = @TableName
  AND column_name = @ColumnName;";

            var exists = Convert.ToInt32(
                db.ExecuteScalar(
                    existsSql,
                    CommandType.Text,
                    new[]
                    {
                        new MySqlParameter("@TableName", tableName),
                        new MySqlParameter("@ColumnName", columnName)
                    })) > 0;

            if (!exists)
            {
                db.ExecuteNonQuery(alterSql, CommandType.Text, null);
            }
        }

        private static void EnsureSemesterRow(DatabaseHelper db, int semesterId, string name, int sortOrder)
        {
            const string sql = @"
INSERT INTO `semester` (`SemesterId`, `Name`, `SortOrder`, `IsActive`)
VALUES (@SemesterId, @Name, @SortOrder, 1)
ON DUPLICATE KEY UPDATE
  `Name` = VALUES(`Name`),
  `SortOrder` = VALUES(`SortOrder`),
  `IsActive` = VALUES(`IsActive`);";

            db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SemesterId", semesterId),
                    new MySqlParameter("@Name", name),
                    new MySqlParameter("@SortOrder", sortOrder)
                });
        }

        private static void EnsureSystemSettingKey(DatabaseHelper db, string key, string defaultValue)
        {
            const string sql = @"
INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES (@Key, @Value)
ON DUPLICATE KEY UPDATE
  `SettingValue` = IFNULL(`SettingValue`, VALUES(`SettingValue`));";

            db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@Key", key),
                    new MySqlParameter("@Value", defaultValue ?? string.Empty)
                });
        }
    }
}

