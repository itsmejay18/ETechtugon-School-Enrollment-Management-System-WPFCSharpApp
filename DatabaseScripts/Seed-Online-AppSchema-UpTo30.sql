-- Top up the live app-schema tables to 30 rows each.
-- Intentionally leaves schemamigration unchanged.
-- systemsetting already has 30 rows in the current deployment.

DELIMITER $$

DROP PROCEDURE IF EXISTS SeedOnlineAppSchemaUpTo30 $$
CREATE PROCEDURE SeedOnlineAppSchemaUpTo30()
BEGIN
    DECLARE v_count INT DEFAULT 0;
    DECLARE v_seq INT DEFAULT 0;

    DECLARE v_current_academic_year_id INT DEFAULT 1;
    DECLARE v_current_semester_id INT DEFAULT 1;

    DECLARE v_department_id INT DEFAULT NULL;
    DECLARE v_course_id INT DEFAULT NULL;
    DECLARE v_yearlevel_id INT DEFAULT NULL;
    DECLARE v_academic_year_id INT DEFAULT NULL;
    DECLARE v_semester_id INT DEFAULT NULL;
    DECLARE v_faculty_id INT DEFAULT NULL;
    DECLARE v_student_id INT DEFAULT NULL;
    DECLARE v_section_id INT DEFAULT NULL;
    DECLARE v_subject_id INT DEFAULT NULL;
    DECLARE v_curriculum_id INT DEFAULT NULL;
    DECLARE v_enrollment_id INT DEFAULT NULL;
    DECLARE v_classschedule_id INT DEFAULT NULL;
    DECLARE v_user_id INT DEFAULT NULL;

    DECLARE v_course_code VARCHAR(20) DEFAULT NULL;
    DECLARE v_yearlevel_name VARCHAR(30) DEFAULT NULL;
    DECLARE v_semester_name VARCHAR(30) DEFAULT NULL;
    DECLARE v_academic_year_name VARCHAR(20) DEFAULT NULL;
    DECLARE v_password_hash VARBINARY(32);
    DECLARE v_password_salt VARBINARY(16);

    SET v_password_hash = UNHEX('491B306728DB9AB65DC7504B3EF8DA1293693F2E55A958405C4BB7BB6672A934');
    SET v_password_salt = UNHEX('2C8FA907A42255A3FB0F3C02B49C7473');

    SELECT COALESCE(
        (SELECT CAST(SettingValue AS UNSIGNED) FROM systemsetting WHERE SettingKey = 'CurrentAcademicYearId' LIMIT 1),
        (SELECT AcademicYearId FROM academicyear WHERE IsCurrent = 1 ORDER BY AcademicYearId LIMIT 1),
        1
    ) INTO v_current_academic_year_id;

    SELECT COALESCE(
        (SELECT CAST(SettingValue AS UNSIGNED) FROM systemsetting WHERE SettingKey = 'CurrentSemesterId' LIMIT 1),
        (SELECT SemesterId FROM semester ORDER BY SortOrder, SemesterId LIMIT 1),
        1
    ) INTO v_current_semester_id;

    -- Academic years
    SELECT COUNT(*) INTO v_count FROM academicyear;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO academicyear (Name, StartDate, EndDate, IsCurrent, IsActive)
        VALUES (
            CONCAT(2024 + v_seq, '-', 2025 + v_seq),
            STR_TO_DATE(CONCAT(2024 + v_seq, '-06-01'), '%Y-%m-%d'),
            STR_TO_DATE(CONCAT(2025 + v_seq, '-05-31'), '%Y-%m-%d'),
            0,
            1
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Semesters
    SELECT COUNT(*) INTO v_count FROM semester;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO semester (Name, SortOrder, IsActive)
        VALUES (CONCAT('Term ', LPAD(v_seq, 2, '0')), v_seq, 1);
        SET v_count = v_count + 1;
    END WHILE;

    -- Year levels
    SELECT COUNT(*) INTO v_count FROM yearlevel;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO yearlevel (Name, SortOrder, IsActive)
        VALUES (CONCAT('Level ', LPAD(v_seq, 2, '0')), v_seq, 1);
        SET v_count = v_count + 1;
    END WHILE;

    -- Departments
    SELECT COUNT(*) INTO v_count FROM department;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            CONCAT('DEP', LPAD(v_seq, 3, '0')),
            CONCAT('Generated Department ', LPAD(v_seq, 2, '0')),
            'Generated online seed data',
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Courses
    SELECT COUNT(*) INTO v_count FROM course;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT d.DepartmentId
        INTO v_department_id
        FROM department d
        LEFT JOIN course c ON c.DepartmentId = d.DepartmentId
        GROUP BY d.DepartmentId
        ORDER BY COUNT(c.CourseId), d.DepartmentId
        LIMIT 1;

        INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
        VALUES (
            CONCAT('CRS', LPAD(v_seq, 3, '0')),
            CONCAT('Generated Course ', LPAD(v_seq, 2, '0')),
            'Generated online seed data',
            v_department_id,
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Faculty
    SELECT COUNT(*) INTO v_count FROM faculty;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO faculty
        (
            FacultyCode, FirstName, LastName, MiddleName, Email, Phone, Address,
            PhotoPath, HireDate, IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT('FAC-2026-', LPAD(v_seq, 4, '0')),
            CONCAT('Faculty', LPAD(v_seq, 2, '0')),
            'Generated',
            NULL,
            CONCAT('faculty', LPAD(v_seq, 2, '0'), '@demo.school'),
            CONCAT('0917', LPAD(v_seq, 7, '0')),
            CONCAT('Generated Faculty Address ', LPAD(v_seq, 2, '0')),
            NULL,
            DATE_SUB(CURDATE(), INTERVAL 30 + v_seq DAY),
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Students
    SELECT COUNT(*) INTO v_count FROM student;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO student
        (
            StudentNumber, FirstName, LastName, MiddleName, Gender, BirthDate, Email,
            Phone, Address, PhotoPath, IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT('STU-2026-', LPAD(v_seq, 4, '0')),
            CONCAT('Student', LPAD(v_seq, 2, '0')),
            'Generated',
            CONCAT('M', LPAD(v_seq, 2, '0')),
            IF(MOD(v_seq, 2) = 0, 'Female', 'Male'),
            DATE_SUB('2005-06-15', INTERVAL v_seq DAY),
            CONCAT('student', LPAD(v_seq, 2, '0'), '@demo.school'),
            CONCAT('0991', LPAD(v_seq, 7, '0')),
            CONCAT('Generated Student Address ', LPAD(v_seq, 2, '0')),
            NULL,
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Users
    SELECT COUNT(*) INTO v_count FROM users;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;
        INSERT INTO users
        (
            Username, PasswordHash, PasswordSalt, Role, DisplayName,
            PhotoPath, IsActive, CreatedAt, UpdatedAt, LastLoginAt
        )
        VALUES
        (
            CONCAT('seed.user', LPAD(v_seq, 2, '0')),
            v_password_hash,
            v_password_salt,
            CASE MOD(v_seq, 3)
                WHEN 0 THEN 'Admin'
                WHEN 1 THEN 'Registrar'
                ELSE 'Faculty'
            END,
            CONCAT('Seed User ', LPAD(v_seq, 2, '0')),
            NULL,
            0,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Subjects
    SELECT COUNT(*) INTO v_count FROM subject;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT c.CourseId
        INTO v_course_id
        FROM course c
        LEFT JOIN subject s ON s.CourseId = c.CourseId
        GROUP BY c.CourseId
        ORDER BY COUNT(s.SubjectId), c.CourseId
        LIMIT 1;

        INSERT INTO subject
        (
            SubjectCode, SubjectName, Units, CourseId, IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT('SUB', LPAD(v_seq, 3, '0')),
            CONCAT('Generated Subject ', LPAD(v_seq, 2, '0')),
            IF(MOD(v_seq, 4) = 0, 4, 3),
            v_course_id,
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Sections
    SELECT COUNT(*) INTO v_count FROM section;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT c.CourseId
        INTO v_course_id
        FROM course c
        LEFT JOIN section s
            ON s.CourseId = c.CourseId
           AND s.AcademicYearId = v_current_academic_year_id
           AND s.SemesterId = v_current_semester_id
        GROUP BY c.CourseId
        ORDER BY COUNT(s.SectionId), c.CourseId
        LIMIT 1;

        SELECT yl.YearLevelId
        INTO v_yearlevel_id
        FROM yearlevel yl
        LEFT JOIN section s
            ON s.YearLevelId = yl.YearLevelId
           AND s.AcademicYearId = v_current_academic_year_id
           AND s.SemesterId = v_current_semester_id
        GROUP BY yl.YearLevelId
        ORDER BY COUNT(s.SectionId), yl.YearLevelId
        LIMIT 1;

        INSERT INTO section
        (
            SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId,
            Capacity, IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT('SEC-', LPAD(v_seq, 3, '0')),
            v_course_id,
            v_yearlevel_id,
            v_current_academic_year_id,
            v_current_semester_id,
            35 + MOD(v_seq, 15),
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Read current labels after top-up
    SELECT Name INTO v_semester_name
    FROM semester
    WHERE SemesterId = v_current_semester_id
    LIMIT 1;

    SELECT Name INTO v_academic_year_name
    FROM academicyear
    WHERE AcademicYearId = v_current_academic_year_id
    LIMIT 1;

    -- Curricula
    SELECT COUNT(*) INTO v_count FROM curriculum;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT c.CourseId, c.CourseCode, yl.YearLevelId, yl.Name
        INTO v_course_id, v_course_code, v_yearlevel_id, v_yearlevel_name
        FROM course c
        CROSS JOIN yearlevel yl
        LEFT JOIN curriculum cur
            ON cur.CourseId = c.CourseId
           AND cur.YearLevelId = yl.YearLevelId
           AND cur.SemesterId = v_current_semester_id
           AND cur.AcademicYearId = v_current_academic_year_id
        WHERE cur.CurriculumId IS NULL
        ORDER BY c.CourseId, yl.YearLevelId
        LIMIT 1;

        INSERT INTO curriculum
        (
            Name, CourseId, YearLevelId, SemesterId, AcademicYearId,
            IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT(v_course_code, ' - ', v_yearlevel_name, ' - ', v_semester_name, ' - ', v_academic_year_name),
            v_course_id,
            v_yearlevel_id,
            v_current_semester_id,
            v_current_academic_year_id,
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Curriculum details
    SELECT COUNT(*) INTO v_count FROM curriculumdetails;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT cur.CurriculumId, sub.SubjectId
        INTO v_curriculum_id, v_subject_id
        FROM curriculum cur
        INNER JOIN subject sub ON sub.CourseId = cur.CourseId
        LEFT JOIN curriculumdetails cd
            ON cd.CurriculumId = cur.CurriculumId
           AND cd.SubjectId = sub.SubjectId
        WHERE cd.CurriculumDetailId IS NULL
        ORDER BY cur.CurriculumId, sub.SubjectId
        LIMIT 1;

        INSERT INTO curriculumdetails (CurriculumId, SubjectId, CreatedAt)
        VALUES (v_curriculum_id, v_subject_id, UTC_TIMESTAMP() - INTERVAL v_seq DAY);
        SET v_count = v_count + 1;
    END WHILE;

    -- Class schedules
    SELECT COUNT(*) INTO v_count FROM classschedule;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT sec.SectionId, sub.SubjectId, sec.AcademicYearId, sec.SemesterId
        INTO v_section_id, v_subject_id, v_academic_year_id, v_semester_id
        FROM section sec
        INNER JOIN subject sub ON sub.CourseId = sec.CourseId
        LEFT JOIN classschedule cs
            ON cs.SectionId = sec.SectionId
           AND cs.SubjectId = sub.SubjectId
           AND cs.AcademicYearId = sec.AcademicYearId
           AND cs.SemesterId = sec.SemesterId
        WHERE cs.ClassScheduleId IS NULL
        ORDER BY sec.SectionId, sub.SubjectId
        LIMIT 1;

        SELECT f.FacultyId
        INTO v_faculty_id
        FROM faculty f
        LEFT JOIN classschedule cs ON cs.FacultyId = f.FacultyId
        GROUP BY f.FacultyId
        ORDER BY COUNT(cs.ClassScheduleId), f.FacultyId
        LIMIT 1;

        INSERT INTO classschedule
        (
            SectionId, SubjectId, FacultyId, DayOfWeek, StartTime, EndTime, Room,
            Remarks, AcademicYearId, SemesterId, IsActive, CreatedAt, UpdatedAt
        )
        VALUES
        (
            v_section_id,
            v_subject_id,
            v_faculty_id,
            CASE MOD(v_seq, 5)
                WHEN 0 THEN 'Monday'
                WHEN 1 THEN 'Tuesday'
                WHEN 2 THEN 'Wednesday'
                WHEN 3 THEN 'Thursday'
                ELSE 'Friday'
            END,
            MAKETIME(7 + MOD(v_seq, 6), 0, 0),
            MAKETIME(8 + MOD(v_seq, 6), 30, 0),
            CONCAT('Room ', 100 + MOD(v_seq, 20)),
            'Generated online seed schedule',
            v_academic_year_id,
            v_semester_id,
            1,
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Enrollments
    SELECT COUNT(*) INTO v_count FROM enrollment;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT st.StudentId
        INTO v_student_id
        FROM student st
        LEFT JOIN enrollment e
            ON e.StudentId = st.StudentId
           AND e.AcademicYearId = v_current_academic_year_id
           AND e.SemesterId = v_current_semester_id
        GROUP BY st.StudentId
        ORDER BY COUNT(e.EnrollmentId), st.StudentId
        LIMIT 1;

        SELECT sec.SectionId, sec.CourseId, sec.YearLevelId, sec.AcademicYearId, sec.SemesterId
        INTO v_section_id, v_course_id, v_yearlevel_id, v_academic_year_id, v_semester_id
        FROM section sec
        LEFT JOIN enrollment e
            ON e.SectionId = sec.SectionId
           AND e.AcademicYearId = sec.AcademicYearId
           AND e.SemesterId = sec.SemesterId
        GROUP BY sec.SectionId, sec.CourseId, sec.YearLevelId, sec.AcademicYearId, sec.SemesterId
        ORDER BY COUNT(e.EnrollmentId), sec.SectionId
        LIMIT 1;

        INSERT INTO enrollment
        (
            EnrollmentNumber, StudentId, CourseId, AcademicYearId, YearLevelId, SemesterId,
            SectionId, EnrollDate, TotalUnits, Status, CreatedAt, UpdatedAt
        )
        VALUES
        (
            CONCAT('ENR-', YEAR(CURDATE()), '-', LPAD(v_seq, 5, '0')),
            v_student_id,
            v_course_id,
            v_academic_year_id,
            v_yearlevel_id,
            v_semester_id,
            v_section_id,
            DATE_SUB(CURDATE(), INTERVAL MOD(v_seq, 30) DAY),
            0,
            'Registered',
            UTC_TIMESTAMP() - INTERVAL v_seq DAY,
            NULL
        );
        SET v_count = v_count + 1;
    END WHILE;

    -- Enrollment details
    SELECT COUNT(*) INTO v_count FROM enrollmentdetails;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT e.EnrollmentId, sub.SubjectId,
               (
                   SELECT cs.ClassScheduleId
                   FROM classschedule cs
                   WHERE cs.SectionId = e.SectionId
                     AND cs.SubjectId = sub.SubjectId
                     AND cs.AcademicYearId = e.AcademicYearId
                     AND cs.SemesterId = e.SemesterId
                   ORDER BY cs.ClassScheduleId
                   LIMIT 1
               )
        INTO v_enrollment_id, v_subject_id, v_classschedule_id
        FROM enrollment e
        INNER JOIN subject sub ON sub.CourseId = e.CourseId
        LEFT JOIN enrollmentdetails ed
            ON ed.EnrollmentId = e.EnrollmentId
           AND ed.SubjectId = sub.SubjectId
        WHERE ed.EnrollmentDetailId IS NULL
        ORDER BY e.EnrollmentId, sub.SubjectId
        LIMIT 1;

        INSERT INTO enrollmentdetails
        (
            EnrollmentId, SubjectId, Units, ClassScheduleId, Grade, CreatedAt
        )
        SELECT
            v_enrollment_id,
            v_subject_id,
            s.Units,
            v_classschedule_id,
            ROUND(82 + MOD(v_seq, 10) * 0.25, 2),
            UTC_TIMESTAMP() - INTERVAL v_seq DAY
        FROM subject s
        WHERE s.SubjectId = v_subject_id
        LIMIT 1;

        SET v_count = v_count + 1;
    END WHILE;

    UPDATE enrollment e
    SET e.TotalUnits = (
        SELECT IFNULL(SUM(ed.Units), 0)
        FROM enrollmentdetails ed
        WHERE ed.EnrollmentId = e.EnrollmentId
    ),
    e.UpdatedAt = UTC_TIMESTAMP();

    -- Activity logs
    SELECT COUNT(*) INTO v_count FROM activitylog;
    WHILE v_count < 30 DO
        SET v_seq = v_count + 1;

        SELECT u.UserId
        INTO v_user_id
        FROM users u
        LEFT JOIN activitylog a ON a.UserId = u.UserId
        GROUP BY u.UserId
        ORDER BY COUNT(a.ActivityLogId), u.UserId
        LIMIT 1;

        INSERT INTO activitylog
        (
            UserId, Action, Entity, EntityId, Details, MachineName, CreatedAt
        )
        VALUES
        (
            v_user_id,
            'SeedData',
            'OnlineSeeder',
            v_seq,
            CONCAT('Generated online seed log #', LPAD(v_seq, 3, '0')),
            'HOSTINGER',
            UTC_TIMESTAMP() - INTERVAL v_seq MINUTE
        );
        SET v_count = v_count + 1;
    END WHILE;
END $$

DELIMITER ;

START TRANSACTION;
CALL SeedOnlineAppSchemaUpTo30();
COMMIT;

DROP PROCEDURE IF EXISTS SeedOnlineAppSchemaUpTo30;

SELECT 'academicyear' AS TableName, COUNT(*) AS RowCount FROM academicyear
UNION ALL SELECT 'activitylog', COUNT(*) FROM activitylog
UNION ALL SELECT 'classschedule', COUNT(*) FROM classschedule
UNION ALL SELECT 'course', COUNT(*) FROM course
UNION ALL SELECT 'curriculum', COUNT(*) FROM curriculum
UNION ALL SELECT 'curriculumdetails', COUNT(*) FROM curriculumdetails
UNION ALL SELECT 'department', COUNT(*) FROM department
UNION ALL SELECT 'enrollment', COUNT(*) FROM enrollment
UNION ALL SELECT 'enrollmentdetails', COUNT(*) FROM enrollmentdetails
UNION ALL SELECT 'faculty', COUNT(*) FROM faculty
UNION ALL SELECT 'schemamigration', COUNT(*) FROM schemamigration
UNION ALL SELECT 'section', COUNT(*) FROM section
UNION ALL SELECT 'semester', COUNT(*) FROM semester
UNION ALL SELECT 'student', COUNT(*) FROM student
UNION ALL SELECT 'subject', COUNT(*) FROM subject
UNION ALL SELECT 'systemsetting', COUNT(*) FROM systemsetting
UNION ALL SELECT 'users', COUNT(*) FROM users
UNION ALL SELECT 'yearlevel', COUNT(*) FROM yearlevel
ORDER BY TableName;
