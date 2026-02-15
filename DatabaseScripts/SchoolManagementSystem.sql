-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: schoolmanagementsystem
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `academicyear`
--

DROP TABLE IF EXISTS `academicyear`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `academicyear` (
  `AcademicYearId` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(20) NOT NULL,
  `StartDate` date DEFAULT NULL,
  `EndDate` date DEFAULT NULL,
  `IsCurrent` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`AcademicYearId`),
  UNIQUE KEY `UX_AcademicYear_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `academicyear`
--

LOCK TABLES `academicyear` WRITE;
/*!40000 ALTER TABLE `academicyear` DISABLE KEYS */;
INSERT INTO `academicyear` VALUES (1,'2025-2026','2025-06-01','2026-05-31',1,1);
/*!40000 ALTER TABLE `academicyear` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `activitylog`
--

DROP TABLE IF EXISTS `activitylog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `activitylog` (
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `activitylog`
--

LOCK TABLES `activitylog` WRITE;
/*!40000 ALTER TABLE `activitylog` DISABLE KEYS */;
/*!40000 ALTER TABLE `activitylog` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `course`
--

DROP TABLE IF EXISTS `course`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `course` (
  `CourseId` int NOT NULL AUTO_INCREMENT,
  `CourseCode` varchar(20) NOT NULL,
  `CourseName` varchar(100) NOT NULL,
  `Description` varchar(250) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`CourseId`),
  UNIQUE KEY `UX_Course_CourseCode` (`CourseCode`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `course`
--

LOCK TABLES `course` WRITE;
/*!40000 ALTER TABLE `course` DISABLE KEYS */;
INSERT INTO `course` VALUES (1,'BSCS','BS Computer Science','Sample course',1,'2026-02-15 14:06:21',NULL),(2,'BSIT','BS Information Technology','Sample course',1,'2026-02-15 14:06:21',NULL);
/*!40000 ALTER TABLE `course` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `curriculum`
--

DROP TABLE IF EXISTS `curriculum`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `curriculum` (
  `CurriculumId` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `CourseId` int NOT NULL,
  `YearLevelId` int NOT NULL,
  `SemesterId` int NOT NULL,
  `AcademicYearId` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`CurriculumId`),
  UNIQUE KEY `UX_Curriculum_Unique` (`CourseId`,`YearLevelId`,`SemesterId`,`AcademicYearId`),
  KEY `FK_Curriculum_YearLevel` (`YearLevelId`),
  KEY `FK_Curriculum_Semester` (`SemesterId`),
  KEY `FK_Curriculum_AcademicYear` (`AcademicYearId`),
  CONSTRAINT `FK_Curriculum_AcademicYear` FOREIGN KEY (`AcademicYearId`) REFERENCES `academicyear` (`AcademicYearId`),
  CONSTRAINT `FK_Curriculum_Course` FOREIGN KEY (`CourseId`) REFERENCES `course` (`CourseId`),
  CONSTRAINT `FK_Curriculum_Semester` FOREIGN KEY (`SemesterId`) REFERENCES `semester` (`SemesterId`),
  CONSTRAINT `FK_Curriculum_YearLevel` FOREIGN KEY (`YearLevelId`) REFERENCES `yearlevel` (`YearLevelId`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `curriculum`
--

LOCK TABLES `curriculum` WRITE;
/*!40000 ALTER TABLE `curriculum` DISABLE KEYS */;
INSERT INTO `curriculum` VALUES (1,'BSCS - BS Computer Science - 1st Year - 1st Semester - 2025-2026',1,1,1,1,1,'2026-02-15 14:32:35',NULL);
/*!40000 ALTER TABLE `curriculum` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `curriculumdetails`
--

DROP TABLE IF EXISTS `curriculumdetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `curriculumdetails` (
  `CurriculumDetailId` int NOT NULL AUTO_INCREMENT,
  `CurriculumId` int NOT NULL,
  `SubjectId` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`CurriculumDetailId`),
  UNIQUE KEY `UX_CurriculumDetails_Unique` (`CurriculumId`,`SubjectId`),
  KEY `FK_CurriculumDetails_Subject` (`SubjectId`),
  CONSTRAINT `FK_CurriculumDetails_Curriculum` FOREIGN KEY (`CurriculumId`) REFERENCES `curriculum` (`CurriculumId`) ON DELETE CASCADE,
  CONSTRAINT `FK_CurriculumDetails_Subject` FOREIGN KEY (`SubjectId`) REFERENCES `subject` (`SubjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `curriculumdetails`
--

LOCK TABLES `curriculumdetails` WRITE;
/*!40000 ALTER TABLE `curriculumdetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `curriculumdetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `enrollment`
--

DROP TABLE IF EXISTS `enrollment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `enrollment` (
  `EnrollmentId` int NOT NULL AUTO_INCREMENT,
  `EnrollmentNumber` varchar(30) NOT NULL,
  `StudentId` int NOT NULL,
  `CourseId` int NOT NULL,
  `AcademicYearId` int NOT NULL,
  `YearLevelId` int NOT NULL,
  `SemesterId` int NOT NULL,
  `EnrollDate` date NOT NULL,
  `TotalUnits` int NOT NULL DEFAULT '0',
  `Status` varchar(20) NOT NULL DEFAULT 'Draft',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`EnrollmentId`),
  UNIQUE KEY `UX_Enrollment_EnrollmentNumber` (`EnrollmentNumber`),
  KEY `IX_Enrollment_StudentId` (`StudentId`),
  KEY `FK_Enrollment_Course` (`CourseId`),
  KEY `FK_Enrollment_AcademicYear` (`AcademicYearId`),
  KEY `FK_Enrollment_YearLevel` (`YearLevelId`),
  KEY `FK_Enrollment_Semester` (`SemesterId`),
  CONSTRAINT `FK_Enrollment_AcademicYear` FOREIGN KEY (`AcademicYearId`) REFERENCES `academicyear` (`AcademicYearId`),
  CONSTRAINT `FK_Enrollment_Course` FOREIGN KEY (`CourseId`) REFERENCES `course` (`CourseId`),
  CONSTRAINT `FK_Enrollment_Semester` FOREIGN KEY (`SemesterId`) REFERENCES `semester` (`SemesterId`),
  CONSTRAINT `FK_Enrollment_Student` FOREIGN KEY (`StudentId`) REFERENCES `student` (`StudentId`),
  CONSTRAINT `FK_Enrollment_YearLevel` FOREIGN KEY (`YearLevelId`) REFERENCES `yearlevel` (`YearLevelId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `enrollment`
--

LOCK TABLES `enrollment` WRITE;
/*!40000 ALTER TABLE `enrollment` DISABLE KEYS */;
/*!40000 ALTER TABLE `enrollment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `enrollmentdetails`
--

DROP TABLE IF EXISTS `enrollmentdetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `enrollmentdetails` (
  `EnrollmentDetailId` int NOT NULL AUTO_INCREMENT,
  `EnrollmentId` int NOT NULL,
  `SubjectId` int NOT NULL,
  `Units` int NOT NULL,
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  PRIMARY KEY (`EnrollmentDetailId`),
  UNIQUE KEY `UX_EnrollmentDetails_Unique` (`EnrollmentId`,`SubjectId`),
  KEY `FK_EnrollmentDetails_Subject` (`SubjectId`),
  CONSTRAINT `FK_EnrollmentDetails_Enrollment` FOREIGN KEY (`EnrollmentId`) REFERENCES `enrollment` (`EnrollmentId`) ON DELETE CASCADE,
  CONSTRAINT `FK_EnrollmentDetails_Subject` FOREIGN KEY (`SubjectId`) REFERENCES `subject` (`SubjectId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `enrollmentdetails`
--

LOCK TABLES `enrollmentdetails` WRITE;
/*!40000 ALTER TABLE `enrollmentdetails` DISABLE KEYS */;
/*!40000 ALTER TABLE `enrollmentdetails` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `faculty`
--

DROP TABLE IF EXISTS `faculty`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `faculty` (
  `FacultyId` int NOT NULL AUTO_INCREMENT,
  `FacultyCode` varchar(20) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Phone` varchar(30) DEFAULT NULL,
  `Address` varchar(250) DEFAULT NULL,
  `HireDate` date DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`FacultyId`),
  UNIQUE KEY `UX_Faculty_FacultyCode` (`FacultyCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `faculty`
--

LOCK TABLES `faculty` WRITE;
/*!40000 ALTER TABLE `faculty` DISABLE KEYS */;
/*!40000 ALTER TABLE `faculty` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `semester`
--

DROP TABLE IF EXISTS `semester`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `semester` (
  `SemesterId` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(30) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`SemesterId`),
  UNIQUE KEY `UX_Semester_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `semester`
--

LOCK TABLES `semester` WRITE;
/*!40000 ALTER TABLE `semester` DISABLE KEYS */;
INSERT INTO `semester` VALUES (1,'1st Semester',1,1),(2,'2nd Semester',2,1);
/*!40000 ALTER TABLE `semester` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `student`
--

DROP TABLE IF EXISTS `student`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `student` (
  `StudentId` int NOT NULL AUTO_INCREMENT,
  `StudentNumber` varchar(30) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT NULL,
  `Gender` varchar(20) DEFAULT NULL,
  `BirthDate` date DEFAULT NULL,
  `Email` varchar(100) DEFAULT NULL,
  `Phone` varchar(30) DEFAULT NULL,
  `Address` varchar(250) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`StudentId`),
  UNIQUE KEY `UX_Student_StudentNumber` (`StudentNumber`),
  KEY `IX_Student_LastFirst` (`LastName`,`FirstName`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `student`
--

LOCK TABLES `student` WRITE;
/*!40000 ALTER TABLE `student` DISABLE KEYS */;
INSERT INTO `student` VALUES (1,'STU-2026-0001','Jay','Ababon','Jemino','Male','2004-06-18','jayjeminoababon@gmail.com','09912268122','Prk. 6, Lower Balutakay, Hagonoy, Davao Del Sur',1,'2026-02-15 15:37:01',NULL);
/*!40000 ALTER TABLE `student` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `subject`
--

DROP TABLE IF EXISTS `subject`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `subject` (
  `SubjectId` int NOT NULL AUTO_INCREMENT,
  `SubjectCode` varchar(20) NOT NULL,
  `SubjectName` varchar(100) NOT NULL,
  `Units` int NOT NULL,
  `CourseId` int DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`SubjectId`),
  UNIQUE KEY `UX_Subject_SubjectCode` (`SubjectCode`),
  KEY `IX_Subject_CourseId` (`CourseId`),
  CONSTRAINT `FK_Subject_Course` FOREIGN KEY (`CourseId`) REFERENCES `course` (`CourseId`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `subject`
--

LOCK TABLES `subject` WRITE;
/*!40000 ALTER TABLE `subject` DISABLE KEYS */;
INSERT INTO `subject` VALUES (1,'CS101','Introduction to Computing',3,1,1,'2026-02-15 14:06:21',NULL),(2,'CS102','Programming Fundamentals',3,1,1,'2026-02-15 14:06:21',NULL),(3,'IT101','Computer Applications',3,2,1,'2026-02-15 14:06:21',NULL);
/*!40000 ALTER TABLE `subject` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `UserId` int NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) NOT NULL,
  `PasswordHash` binary(32) NOT NULL,
  `PasswordSalt` binary(16) NOT NULL,
  `Role` varchar(30) NOT NULL,
  `DisplayName` varchar(100) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime NOT NULL DEFAULT (utc_timestamp()),
  `UpdatedAt` datetime DEFAULT NULL,
  `LastLoginAt` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `UX_Users_Username` (`Username`),
  KEY `IX_Users_Role` (`Role`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'admin',_binary 'ñv\”¡à\À\ZX˛@hC\÷\ÕPà3\ÌGΩ$NFH\"\ZÚ\Î',_binary 'F\·˚Ü6*˛¥∑%ú\\ ñ','Admin','System Administrator',1,'2026-02-15 14:06:21',NULL,'2026-02-15 16:21:32'),(2,'registrar',_binary 'Æç \–EπÒS±ztG‘≤`ä^∏≠˘»ç8?',_binary '-èÜÛæµC	Û]s\Íu¢','Registrar','Default Registrar',1,'2026-02-15 14:06:21',NULL,NULL),(3,'faculty1',_binary '\«|{∫\„≤ˇRAπ¿	t\Í⁄ç°•xó?p∂\"gº\\\√S',_binary 'H†Âí´(\Î\”V\ﬂJ¶\Áàj_','Faculty','Default Faculty',1,'2026-02-15 14:06:21',NULL,NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `yearlevel`
--

DROP TABLE IF EXISTS `yearlevel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `yearlevel` (
  `YearLevelId` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(30) NOT NULL,
  `SortOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`YearLevelId`),
  UNIQUE KEY `UX_YearLevel_Name` (`Name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `yearlevel`
--

LOCK TABLES `yearlevel` WRITE;
/*!40000 ALTER TABLE `yearlevel` DISABLE KEYS */;
INSERT INTO `yearlevel` VALUES (1,'1st Year',1,1),(2,'2nd Year',2,1),(3,'3rd Year',3,1),(4,'4th Year',4,1);
/*!40000 ALTER TABLE `yearlevel` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-16  0:25:19
