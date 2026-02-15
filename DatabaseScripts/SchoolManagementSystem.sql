/*
  School Management System Database Script
  Target: MySQL 8.0+

  Default Login Accounts:
    Username: admin      Password: admin123
    Username: registrar  Password: registrar123
    Username: faculty1   Password: faculty123
*/

SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS SchoolManagementSystem
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE SchoolManagementSystem;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS EnrollmentDetails;
DROP TABLE IF EXISTS Enrollment;
DROP TABLE IF EXISTS CurriculumDetails;
DROP TABLE IF EXISTS Curriculum;
DROP TABLE IF EXISTS Subject;
DROP TABLE IF EXISTS Course;
DROP TABLE IF EXISTS Student;
DROP TABLE IF EXISTS Faculty;
DROP TABLE IF EXISTS ActivityLog;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS AcademicYear;
DROP TABLE IF EXISTS YearLevel;
DROP TABLE IF EXISTS Semester;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE AcademicYear
(
    AcademicYearId INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(20) NOT NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    IsCurrent TINYINT(1) NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (AcademicYearId),
    UNIQUE KEY UX_AcademicYear_Name (Name)
) ENGINE=InnoDB;

CREATE TABLE YearLevel
(
    YearLevelId INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(30) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (YearLevelId),
    UNIQUE KEY UX_YearLevel_Name (Name)
) ENGINE=InnoDB;

CREATE TABLE Semester
(
    SemesterId INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(30) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (SemesterId),
    UNIQUE KEY UX_Semester_Name (Name)
) ENGINE=InnoDB;

CREATE TABLE Users
(
    UserId INT NOT NULL AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL,
    PasswordHash BINARY(32) NOT NULL,
    PasswordSalt BINARY(16) NOT NULL,
    `Role` VARCHAR(30) NOT NULL,
    DisplayName VARCHAR(100) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    LastLoginAt DATETIME NULL,
    PRIMARY KEY (UserId),
    UNIQUE KEY UX_Users_Username (Username),
    KEY IX_Users_Role (`Role`)
) ENGINE=InnoDB;

CREATE TABLE ActivityLog
(
    ActivityLogId INT NOT NULL AUTO_INCREMENT,
    UserId INT NULL,
    `Action` VARCHAR(50) NOT NULL,
    Entity VARCHAR(50) NULL,
    EntityId INT NULL,
    Details VARCHAR(4000) NULL,
    MachineName VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    PRIMARY KEY (ActivityLogId),
    KEY IX_ActivityLog_CreatedAt (CreatedAt),
    CONSTRAINT FK_ActivityLog_Users FOREIGN KEY (UserId) REFERENCES Users (UserId) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE Faculty
(
    FacultyId INT NOT NULL AUTO_INCREMENT,
    FacultyCode VARCHAR(20) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50) NULL,
    Email VARCHAR(100) NULL,
    Phone VARCHAR(30) NULL,
    Address VARCHAR(250) NULL,
    HireDate DATE NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (FacultyId),
    UNIQUE KEY UX_Faculty_FacultyCode (FacultyCode)
) ENGINE=InnoDB;

CREATE TABLE Course
(
    CourseId INT NOT NULL AUTO_INCREMENT,
    CourseCode VARCHAR(20) NOT NULL,
    CourseName VARCHAR(100) NOT NULL,
    Description VARCHAR(250) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (CourseId),
    UNIQUE KEY UX_Course_CourseCode (CourseCode)
) ENGINE=InnoDB;

CREATE TABLE Subject
(
    SubjectId INT NOT NULL AUTO_INCREMENT,
    SubjectCode VARCHAR(20) NOT NULL,
    SubjectName VARCHAR(100) NOT NULL,
    Units INT NOT NULL,
    CourseId INT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (SubjectId),
    UNIQUE KEY UX_Subject_SubjectCode (SubjectCode),
    KEY IX_Subject_CourseId (CourseId),
    CONSTRAINT FK_Subject_Course FOREIGN KEY (CourseId) REFERENCES Course (CourseId)
) ENGINE=InnoDB;

CREATE TABLE Student
(
    StudentId INT NOT NULL AUTO_INCREMENT,
    StudentNumber VARCHAR(30) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50) NULL,
    Gender VARCHAR(20) NULL,
    BirthDate DATE NULL,
    Email VARCHAR(100) NULL,
    Phone VARCHAR(30) NULL,
    Address VARCHAR(250) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (StudentId),
    UNIQUE KEY UX_Student_StudentNumber (StudentNumber),
    KEY IX_Student_LastFirst (LastName, FirstName)
) ENGINE=InnoDB;

CREATE TABLE Curriculum
(
    CurriculumId INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL,
    CourseId INT NOT NULL,
    YearLevelId INT NOT NULL,
    SemesterId INT NOT NULL,
    AcademicYearId INT NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (CurriculumId),
    UNIQUE KEY UX_Curriculum_Unique (CourseId, YearLevelId, SemesterId, AcademicYearId),
    CONSTRAINT FK_Curriculum_Course FOREIGN KEY (CourseId) REFERENCES Course (CourseId),
    CONSTRAINT FK_Curriculum_YearLevel FOREIGN KEY (YearLevelId) REFERENCES YearLevel (YearLevelId),
    CONSTRAINT FK_Curriculum_Semester FOREIGN KEY (SemesterId) REFERENCES Semester (SemesterId),
    CONSTRAINT FK_Curriculum_AcademicYear FOREIGN KEY (AcademicYearId) REFERENCES AcademicYear (AcademicYearId)
) ENGINE=InnoDB;

CREATE TABLE CurriculumDetails
(
    CurriculumDetailId INT NOT NULL AUTO_INCREMENT,
    CurriculumId INT NOT NULL,
    SubjectId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    PRIMARY KEY (CurriculumDetailId),
    UNIQUE KEY UX_CurriculumDetails_Unique (CurriculumId, SubjectId),
    CONSTRAINT FK_CurriculumDetails_Curriculum FOREIGN KEY (CurriculumId) REFERENCES Curriculum (CurriculumId) ON DELETE CASCADE,
    CONSTRAINT FK_CurriculumDetails_Subject FOREIGN KEY (SubjectId) REFERENCES Subject (SubjectId)
) ENGINE=InnoDB;

CREATE TABLE Enrollment
(
    EnrollmentId INT NOT NULL AUTO_INCREMENT,
    EnrollmentNumber VARCHAR(30) NOT NULL,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    AcademicYearId INT NOT NULL,
    YearLevelId INT NOT NULL,
    SemesterId INT NOT NULL,
    EnrollDate DATE NOT NULL,
    TotalUnits INT NOT NULL DEFAULT 0,
    `Status` VARCHAR(20) NOT NULL DEFAULT 'Draft',
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    UpdatedAt DATETIME NULL,
    PRIMARY KEY (EnrollmentId),
    UNIQUE KEY UX_Enrollment_EnrollmentNumber (EnrollmentNumber),
    KEY IX_Enrollment_StudentId (StudentId),
    CONSTRAINT FK_Enrollment_Student FOREIGN KEY (StudentId) REFERENCES Student (StudentId),
    CONSTRAINT FK_Enrollment_Course FOREIGN KEY (CourseId) REFERENCES Course (CourseId),
    CONSTRAINT FK_Enrollment_AcademicYear FOREIGN KEY (AcademicYearId) REFERENCES AcademicYear (AcademicYearId),
    CONSTRAINT FK_Enrollment_YearLevel FOREIGN KEY (YearLevelId) REFERENCES YearLevel (YearLevelId),
    CONSTRAINT FK_Enrollment_Semester FOREIGN KEY (SemesterId) REFERENCES Semester (SemesterId)
) ENGINE=InnoDB;

CREATE TABLE EnrollmentDetails
(
    EnrollmentDetailId INT NOT NULL AUTO_INCREMENT,
    EnrollmentId INT NOT NULL,
    SubjectId INT NOT NULL,
    Units INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    PRIMARY KEY (EnrollmentDetailId),
    UNIQUE KEY UX_EnrollmentDetails_Unique (EnrollmentId, SubjectId),
    CONSTRAINT FK_EnrollmentDetails_Enrollment FOREIGN KEY (EnrollmentId) REFERENCES Enrollment (EnrollmentId) ON DELETE CASCADE,
    CONSTRAINT FK_EnrollmentDetails_Subject FOREIGN KEY (SubjectId) REFERENCES Subject (SubjectId)
) ENGINE=InnoDB;

INSERT INTO YearLevel (Name, SortOrder) VALUES
('1st Year', 1),
('2nd Year', 2),
('3rd Year', 3),
('4th Year', 4);

INSERT INTO Semester (Name, SortOrder) VALUES
('1st Semester', 1),
('2nd Semester', 2);

INSERT INTO AcademicYear (Name, StartDate, EndDate, IsCurrent) VALUES
('2025-2026', '2025-06-01', '2026-05-31', 1);

INSERT INTO Course (CourseCode, CourseName, Description) VALUES
('BSCS', 'BS Computer Science', 'Sample course'),
('BSIT', 'BS Information Technology', 'Sample course');

INSERT INTO Subject (SubjectCode, SubjectName, Units, CourseId) VALUES
('CS101', 'Introduction to Computing', 3, (SELECT CourseId FROM Course WHERE CourseCode = 'BSCS' LIMIT 1)),
('CS102', 'Programming Fundamentals', 3, (SELECT CourseId FROM Course WHERE CourseCode = 'BSCS' LIMIT 1)),
('IT101', 'Computer Applications', 3, (SELECT CourseId FROM Course WHERE CourseCode = 'BSIT' LIMIT 1));

INSERT INTO Users (Username, PasswordHash, PasswordSalt, `Role`, DisplayName, IsActive)
VALUES
(
    'admin',
    UNHEX('9676d3c1887fcb1a58fe406843d608cd508833ed1347bd19244e4648221af2eb'),
    UNHEX('46e1fb86362afeb4b70816259c5c2096'),
    'Admin',
    'System Administrator',
    1
),
(
    'registrar',
    UNHEX('020eae8d20d045b9f153b17a744715d4b2600c198a5e07b814adf902c88d383f'),
    UNHEX('2d8f86f305beb5430b09f35d73ea75a2'),
    'Registrar',
    'Default Registrar',
    1
),
(
    'faculty1',
    UNHEX('c77c7b0fbae3b2ff5241b9c00974eada8da1a578973f70b6221767bc135cc353'),
    UNHEX('48a0e592ab28ebd356df4aa6e7886a5f'),
    'Faculty',
    'Default Faculty',
    1
);
