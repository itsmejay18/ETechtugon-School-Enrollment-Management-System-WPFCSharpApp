SET @schema_name = DATABASE();

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'users'
          AND column_name = 'PhotoPath'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `users` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `DisplayName`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'faculty'
          AND column_name = 'PhotoPath'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `faculty` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'student'
          AND column_name = 'PhotoPath'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `student` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

SET @sql = IF(
    EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = @schema_name
          AND table_name = 'activitylog'
          AND column_name = 'MachineName'
    ),
    'SET @migration_noop = 1',
    'ALTER TABLE `activitylog` ADD COLUMN `MachineName` varchar(100) DEFAULT NULL AFTER `Details`'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
