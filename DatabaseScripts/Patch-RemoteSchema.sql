USE `schoolmanagementsystem`;

-- Add missing columns required by current app queries.
SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'course'
      AND column_name = 'DepartmentId'
  ),
  'SELECT 1',
  'ALTER TABLE `course` ADD COLUMN `DepartmentId` int DEFAULT NULL AFTER `Description`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'faculty'
      AND column_name = 'PhotoPath'
  ),
  'SELECT 1',
  'ALTER TABLE `faculty` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'users'
      AND column_name = 'PhotoPath'
  ),
  'SELECT 1',
  'ALTER TABLE `users` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `DisplayName`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'student'
      AND column_name = 'PhotoPath'
  ),
  'SELECT 1',
  'ALTER TABLE `student` ADD COLUMN `PhotoPath` varchar(260) DEFAULT NULL AFTER `Address`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'enrollment'
      AND column_name = 'SectionId'
  ),
  'SELECT 1',
  'ALTER TABLE `enrollment` ADD COLUMN `SectionId` int DEFAULT NULL AFTER `SemesterId`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'enrollmentdetails'
      AND column_name = 'ClassScheduleId'
  ),
  'SELECT 1',
  'ALTER TABLE `enrollmentdetails` ADD COLUMN `ClassScheduleId` int DEFAULT NULL AFTER `Units`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := IF(
  EXISTS(
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = DATABASE()
      AND table_name = 'enrollmentdetails'
      AND column_name = 'Grade'
  ),
  'SELECT 1',
  'ALTER TABLE `enrollmentdetails` ADD COLUMN `Grade` decimal(5,2) DEFAULT NULL AFTER `ClassScheduleId`'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Create missing tables (safe, only if absent).
CREATE TABLE IF NOT EXISTS `department` (
  `DepartmentId` int NOT NULL AUTO_INCREMENT,
  `DepartmentCode` varchar(20) NOT NULL,
  `DepartmentName` varchar(100) NOT NULL,
  `Description` varchar(250) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`DepartmentId`),
  UNIQUE KEY `UX_Department_Code` (`DepartmentCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `section` (
  `SectionId` int NOT NULL AUTO_INCREMENT,
  `SectionName` varchar(50) NOT NULL,
  `CourseId` int NOT NULL,
  `YearLevelId` int NOT NULL,
  `AcademicYearId` int NOT NULL,
  `SemesterId` int NOT NULL,
  `Capacity` int DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`SectionId`),
  UNIQUE KEY `UX_Section_Unique` (`SectionName`,`CourseId`,`AcademicYearId`,`SemesterId`),
  KEY `FK_Section_Course` (`CourseId`),
  KEY `FK_Section_YearLevel` (`YearLevelId`),
  KEY `FK_Section_AcademicYear` (`AcademicYearId`),
  KEY `FK_Section_Semester` (`SemesterId`),
  CONSTRAINT `FK_Section_AcademicYear` FOREIGN KEY (`AcademicYearId`) REFERENCES `academicyear` (`AcademicYearId`),
  CONSTRAINT `FK_Section_Course` FOREIGN KEY (`CourseId`) REFERENCES `course` (`CourseId`),
  CONSTRAINT `FK_Section_Semester` FOREIGN KEY (`SemesterId`) REFERENCES `semester` (`SemesterId`),
  CONSTRAINT `FK_Section_YearLevel` FOREIGN KEY (`YearLevelId`) REFERENCES `yearlevel` (`YearLevelId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `classschedule` (
  `ClassScheduleId` int NOT NULL AUTO_INCREMENT,
  `SectionId` int NOT NULL,
  `SubjectId` int NOT NULL,
  `FacultyId` int DEFAULT NULL,
  `DayOfWeek` varchar(40) DEFAULT NULL,
  `StartTime` time DEFAULT NULL,
  `EndTime` time DEFAULT NULL,
  `Room` varchar(50) DEFAULT NULL,
  `Remarks` varchar(250) DEFAULT NULL,
  `AcademicYearId` int NOT NULL,
  `SemesterId` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`ClassScheduleId`),
  UNIQUE KEY `UX_ClassSchedule_SectionSubject` (`SectionId`,`SubjectId`,`AcademicYearId`,`SemesterId`),
  KEY `FK_ClassSchedule_Subject` (`SubjectId`),
  KEY `FK_ClassSchedule_Faculty` (`FacultyId`),
  KEY `FK_ClassSchedule_AcademicYear` (`AcademicYearId`),
  KEY `FK_ClassSchedule_Semester` (`SemesterId`),
  CONSTRAINT `FK_ClassSchedule_AcademicYear` FOREIGN KEY (`AcademicYearId`) REFERENCES `academicyear` (`AcademicYearId`),
  CONSTRAINT `FK_ClassSchedule_Faculty` FOREIGN KEY (`FacultyId`) REFERENCES `faculty` (`FacultyId`) ON DELETE SET NULL,
  CONSTRAINT `FK_ClassSchedule_Semester` FOREIGN KEY (`SemesterId`) REFERENCES `semester` (`SemesterId`),
  CONSTRAINT `FK_ClassSchedule_Section` FOREIGN KEY (`SectionId`) REFERENCES `section` (`SectionId`),
  CONSTRAINT `FK_ClassSchedule_Subject` FOREIGN KEY (`SubjectId`) REFERENCES `subject` (`SubjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS `systemsetting` (
  `SettingKey` varchar(50) NOT NULL,
  `SettingValue` varchar(200) NOT NULL,
  PRIMARY KEY (`SettingKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

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

-- Seed baseline reference values if empty.
INSERT INTO `department`
(`DepartmentCode`, `DepartmentName`, `Description`, `IsActive`, `CreatedAt`, `UpdatedAt`)
SELECT 'CIT', 'College of Information Technology', 'Default department', 1, UTC_TIMESTAMP(), NULL
WHERE NOT EXISTS (SELECT 1 FROM `department`);

SET @deptId := (SELECT `DepartmentId` FROM `department` ORDER BY `DepartmentId` LIMIT 1);
UPDATE `course`
SET `DepartmentId` = @deptId
WHERE `DepartmentId` IS NULL;

SET @ayId := (SELECT `AcademicYearId` FROM `academicyear` WHERE `IsCurrent` = 1 ORDER BY `AcademicYearId` LIMIT 1);
SET @ayId := COALESCE(@ayId, (SELECT `AcademicYearId` FROM `academicyear` ORDER BY `AcademicYearId` LIMIT 1), 1);

SET @semId := (SELECT `SemesterId` FROM `semester` ORDER BY `SortOrder`, `SemesterId` LIMIT 1);
SET @semId := COALESCE(@semId, (SELECT `SemesterId` FROM `semester` ORDER BY `SemesterId` LIMIT 1), 1);

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('CurrentAcademicYearId', CAST(@ayId AS CHAR))
ON DUPLICATE KEY UPDATE `SettingValue` = VALUES(`SettingValue`);

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('CurrentSemesterId', CAST(@semId AS CHAR))
ON DUPLICATE KEY UPDATE `SettingValue` = VALUES(`SettingValue`);

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbConnectionMode', 'Local')
ON DUPLICATE KEY UPDATE `SettingValue` = VALUES(`SettingValue`);

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbHost.Local', 'localhost')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPort.Local', '3306')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbName.Local', 'schoolmanagementsystem')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbUser.Local', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPassword.Local', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbHost.Wired', 'localhost')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPort.Wired', '3306')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbName.Wired', 'schoolmanagementsystem')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbUser.Wired', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPassword.Wired', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbHost.Wireless', 'localhost')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPort.Wireless', '3306')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbName.Wireless', 'schoolmanagementsystem')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbUser.Wireless', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPassword.Wireless', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbHost.Online', 'localhost')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPort.Online', '3306')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbName.Online', 'schoolmanagementsystem')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbUser.Online', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPassword.Online', 'root')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbHost.Network', 'localhost')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbPort.Network', '3306')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('DbName.Network', 'schoolmanagementsystem')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('Backup.Directory', '')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(`SettingValue`, VALUES(`SettingValue`));

INSERT INTO `systemsetting` (`SettingKey`, `SettingValue`)
VALUES ('Backup.PreferredType', 'Full')
ON DUPLICATE KEY UPDATE `SettingValue` = IFNULL(NULLIF(`SettingValue`, ''), VALUES(`SettingValue`));

-- Seed sample sections if none exist.
INSERT INTO `section`
(`SectionName`, `CourseId`, `YearLevelId`, `AcademicYearId`, `SemesterId`, `Capacity`, `IsActive`, `CreatedAt`, `UpdatedAt`)
SELECT
  'Section A',
  c.`CourseId`,
  yl.`YearLevelId`,
  @ayId,
  @semId,
  40,
  1,
  UTC_TIMESTAMP(),
  NULL
FROM (SELECT `CourseId` FROM `course` ORDER BY `CourseId` LIMIT 1) c
CROSS JOIN (SELECT `YearLevelId` FROM `yearlevel` ORDER BY `SortOrder`, `YearLevelId` LIMIT 1) yl
WHERE NOT EXISTS (SELECT 1 FROM `section`);

INSERT INTO `section`
(`SectionName`, `CourseId`, `YearLevelId`, `AcademicYearId`, `SemesterId`, `Capacity`, `IsActive`, `CreatedAt`, `UpdatedAt`)
SELECT
  'Section B',
  c.`CourseId`,
  yl.`YearLevelId`,
  @ayId,
  @semId,
  40,
  1,
  UTC_TIMESTAMP(),
  NULL
FROM (SELECT `CourseId` FROM `course` ORDER BY `CourseId` LIMIT 1 OFFSET 1) c
CROSS JOIN (SELECT `YearLevelId` FROM `yearlevel` ORDER BY `SortOrder`, `YearLevelId` LIMIT 1) yl
WHERE NOT EXISTS (
  SELECT 1
  FROM `section`
  WHERE `SectionName` = 'Section B'
    AND `AcademicYearId` = @ayId
    AND `SemesterId` = @semId
);

-- Seed sample schedules if none exist.
INSERT INTO `classschedule`
(`SectionId`, `SubjectId`, `FacultyId`, `DayOfWeek`, `StartTime`, `EndTime`, `Room`, `Remarks`, `AcademicYearId`, `SemesterId`, `IsActive`, `CreatedAt`, `UpdatedAt`)
SELECT
  s.`SectionId`,
  sb.`SubjectId`,
  NULL,
  'Mon/Wed',
  '08:00:00',
  '09:30:00',
  'Lab 1',
  '',
  @ayId,
  @semId,
  1,
  UTC_TIMESTAMP(),
  NULL
FROM (SELECT `SectionId` FROM `section` ORDER BY `SectionId` LIMIT 1) s
CROSS JOIN (SELECT `SubjectId` FROM `subject` ORDER BY `SubjectId` LIMIT 1) sb
WHERE NOT EXISTS (SELECT 1 FROM `classschedule`);

INSERT INTO `classschedule`
(`SectionId`, `SubjectId`, `FacultyId`, `DayOfWeek`, `StartTime`, `EndTime`, `Room`, `Remarks`, `AcademicYearId`, `SemesterId`, `IsActive`, `CreatedAt`, `UpdatedAt`)
SELECT
  s.`SectionId`,
  sb.`SubjectId`,
  NULL,
  'Tue/Thu',
  '10:00:00',
  '11:30:00',
  'Room 101',
  '',
  @ayId,
  @semId,
  1,
  UTC_TIMESTAMP(),
  NULL
FROM (SELECT `SectionId` FROM `section` ORDER BY `SectionId` LIMIT 1) s
CROSS JOIN (SELECT `SubjectId` FROM `subject` ORDER BY `SubjectId` LIMIT 1 OFFSET 1) sb
WHERE NOT EXISTS (
  SELECT 1
  FROM `classschedule`
  WHERE `SectionId` = s.`SectionId`
    AND `SubjectId` = sb.`SubjectId`
    AND `AcademicYearId` = @ayId
    AND `SemesterId` = @semId
);
