-- Non-destructive upgrade for Student Profile, Colleges, department heads,
-- section codes, curriculum versions, and subject capacity.

CREATE TABLE IF NOT EXISTS `college` (
  `CollegeId` int NOT NULL AUTO_INCREMENT,
  `CollegeCode` varchar(20) NOT NULL,
  `CollegeName` varchar(160) NOT NULL,
  `DeanName` varchar(160) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`CollegeId`),
  UNIQUE KEY `UX_College_Code` (`CollegeCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `college` (`CollegeCode`, `CollegeName`, `DeanName`, `Description`, `IsActive`, `CreatedAt`)
VALUES
  ('CCS', 'College of Computer Studies', NULL, 'Computing, information systems, and technology programs.', 1, UTC_TIMESTAMP()),
  ('CBA', 'College of Business Administration', NULL, 'Business, accountancy, and management programs.', 1, UTC_TIMESTAMP()),
  ('COE', 'College of Education', NULL, 'Teacher education and curriculum programs.', 1, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
  `CollegeName` = VALUES(`CollegeName`),
  `UpdatedAt` = UTC_TIMESTAMP();

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `department` ADD COLUMN `CollegeId` int DEFAULT NULL AFTER `DepartmentId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'department' AND column_name = 'CollegeId'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `department` ADD COLUMN `DepartmentHead` varchar(160) DEFAULT NULL AFTER `DepartmentName`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'department' AND column_name = 'DepartmentHead'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `student` ADD COLUMN `StudentType` varchar(20) NOT NULL DEFAULT ''Regular'' AFTER `StudentNumber`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'student' AND column_name = 'StudentType'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `student` ADD COLUMN `CurriculumId` int DEFAULT NULL AFTER `StudentType`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'student' AND column_name = 'CurriculumId'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `student` ADD COLUMN `AcademicStatus` varchar(40) NOT NULL DEFAULT ''Active'' AFTER `CurriculumId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'student' AND column_name = 'AcademicStatus'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `section` ADD COLUMN `SectionCode` varchar(40) DEFAULT NULL AFTER `SectionId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'section' AND column_name = 'SectionCode'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE `section`
SET `SectionCode` = UPPER(REPLACE(REPLACE(TRIM(`SectionName`), ' ', ''), '-', ''))
WHERE (`SectionCode` IS NULL OR `SectionCode` = '')
  AND `SectionName` IS NOT NULL;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `curriculum` ADD COLUMN `CurriculumType` varchar(10) NOT NULL DEFAULT ''NEW'' AFTER `Name`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'curriculum' AND column_name = 'CurriculumType'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `subject` ADD COLUMN `MaxStudents` int DEFAULT NULL AFTER `CourseId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'subject' AND column_name = 'MaxStudents'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `subject` ADD COLUMN `CurrentEnrolledCount` int NOT NULL DEFAULT 0 AFTER `MaxStudents`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'subject' AND column_name = 'CurrentEnrolledCount'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CREATE TABLE IF NOT EXISTS `subjectprerequisite` (
  `SubjectId` int NOT NULL,
  `PrerequisiteSubjectId` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`SubjectId`, `PrerequisiteSubjectId`),
  KEY `IX_SubjectPrerequisite_Prerequisite` (`PrerequisiteSubjectId`),
  CONSTRAINT `FK_SubjectPrerequisite_Subject` FOREIGN KEY (`SubjectId`) REFERENCES `subject` (`SubjectId`) ON DELETE CASCADE,
  CONSTRAINT `FK_SubjectPrerequisite_Prerequisite` FOREIGN KEY (`PrerequisiteSubjectId`) REFERENCES `subject` (`SubjectId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `enrollment` ADD COLUMN `CurriculumId` int DEFAULT NULL AFTER `SectionId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'enrollment' AND column_name = 'CurriculumId'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @sql := (
  SELECT IF(COUNT(*) = 0,
    'ALTER TABLE `enrollment` ADD COLUMN `StudentType` varchar(20) NOT NULL DEFAULT ''Regular'' AFTER `CurriculumId`',
    'SELECT 1')
  FROM information_schema.columns
  WHERE table_schema = DATABASE() AND table_name = 'enrollment' AND column_name = 'StudentType'
);
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE `subject` s
SET s.`CurrentEnrolledCount` = (
    SELECT COUNT(DISTINCT e.`StudentId`)
    FROM `enrollmentdetails` ed
    INNER JOIN `enrollment` e ON e.`EnrollmentId` = ed.`EnrollmentId`
    WHERE ed.`SubjectId` = s.`SubjectId`
      AND e.`Status` <> 'Cancelled'
);
