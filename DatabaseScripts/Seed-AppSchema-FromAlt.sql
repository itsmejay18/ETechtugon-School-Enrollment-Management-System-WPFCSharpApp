-- Seed app schema tables (singular names) using data already loaded in alternate plural tables.

SET @ayId := COALESCE(
    (SELECT CAST(SettingValue AS UNSIGNED) FROM systemsetting WHERE SettingKey = 'CurrentAcademicYearId' LIMIT 1),
    (SELECT AcademicYearId FROM academicyear WHERE IsCurrent = 1 ORDER BY AcademicYearId LIMIT 1),
    1
);

SET @semId := COALESCE(
    (SELECT CAST(SettingValue AS UNSIGNED) FROM systemsetting WHERE SettingKey = 'CurrentSemesterId' LIMIT 1),
    (SELECT SemesterId FROM semester ORDER BY SortOrder, SemesterId LIMIT 1),
    1
);

-- Departments
INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('CIT', 'College of Information Technology', 'Seeded department', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE DepartmentName = VALUES(DepartmentName), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('CED', 'College of Education', 'Seeded department', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE DepartmentName = VALUES(DepartmentName), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('CBA', 'College of Business and Accountancy', 'Seeded department', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE DepartmentName = VALUES(DepartmentName), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('CCJE', 'College of Criminal Justice Education', 'Seeded department', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE DepartmentName = VALUES(DepartmentName), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO department (DepartmentCode, DepartmentName, Description, IsActive, CreatedAt, UpdatedAt)
VALUES ('CON', 'College of Nursing', 'Seeded department', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE DepartmentName = VALUES(DepartmentName), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

-- Courses (app table: course)
INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSIT', 'BS Information Technology', 'Seeded production data', d.DepartmentId, 1, UTC_TIMESTAMP(), NULL
FROM department d
WHERE d.DepartmentCode = 'CIT'
ON DUPLICATE KEY UPDATE CourseName = VALUES(CourseName), Description = VALUES(Description), DepartmentId = VALUES(DepartmentId), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSED', 'BS Secondary Education', 'Seeded production data', d.DepartmentId, 1, UTC_TIMESTAMP(), NULL
FROM department d
WHERE d.DepartmentCode = 'CED'
ON DUPLICATE KEY UPDATE CourseName = VALUES(CourseName), Description = VALUES(Description), DepartmentId = VALUES(DepartmentId), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSBA', 'BS Business Administration', 'Seeded production data', d.DepartmentId, 1, UTC_TIMESTAMP(), NULL
FROM department d
WHERE d.DepartmentCode = 'CBA'
ON DUPLICATE KEY UPDATE CourseName = VALUES(CourseName), Description = VALUES(Description), DepartmentId = VALUES(DepartmentId), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSCRIM', 'BS Criminology', 'Seeded production data', d.DepartmentId, 1, UTC_TIMESTAMP(), NULL
FROM department d
WHERE d.DepartmentCode = 'CCJE'
ON DUPLICATE KEY UPDATE CourseName = VALUES(CourseName), Description = VALUES(Description), DepartmentId = VALUES(DepartmentId), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

INSERT INTO course (CourseCode, CourseName, Description, DepartmentId, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSN', 'BS Nursing', 'Seeded production data', d.DepartmentId, 1, UTC_TIMESTAMP(), NULL
FROM department d
WHERE d.DepartmentCode = 'CON'
ON DUPLICATE KEY UPDATE CourseName = VALUES(CourseName), Description = VALUES(Description), DepartmentId = VALUES(DepartmentId), IsActive = 1, UpdatedAt = UTC_TIMESTAMP();

-- Sections (app table: section)
INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSIT 1A', c.CourseId, 1, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSIT'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSIT 1A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSIT 2A', c.CourseId, 2, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSIT'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSIT 2A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSEd 1A', c.CourseId, 1, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSED'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSEd 1A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSEd 2A', c.CourseId, 2, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSED'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSEd 2A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSBA 1A', c.CourseId, 1, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSBA'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSBA 1A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSBA 2A', c.CourseId, 2, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSBA'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSBA 2A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSCrim 1A', c.CourseId, 1, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSCRIM'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSCrim 1A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSCrim 2A', c.CourseId, 2, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSCRIM'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSCrim 2A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSN 1A', c.CourseId, 1, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSN'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSN 1A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

INSERT INTO section (SectionName, CourseId, YearLevelId, AcademicYearId, SemesterId, Capacity, IsActive, CreatedAt, UpdatedAt)
SELECT 'BSN 2A', c.CourseId, 2, @ayId, @semId, 45, 1, UTC_TIMESTAMP(), NULL
FROM course c
WHERE c.CourseCode = 'BSN'
  AND NOT EXISTS (
      SELECT 1 FROM section s WHERE s.SectionName = 'BSN 2A' AND s.CourseId = c.CourseId AND s.AcademicYearId = @ayId AND s.SemesterId = @semId
  );

-- Faculty from teachers
INSERT INTO faculty
(
  FacultyCode, FirstName, LastName, MiddleName, Email, Phone, Address, HireDate, IsActive, CreatedAt, UpdatedAt
)
SELECT
  CONCAT('FAC-2026-', LPAD(1000 + t.teacher_id, 4, '0')),
  t.first_name,
  t.last_name,
  NULL,
  LOWER(REPLACE(CONCAT(t.first_name, '.', t.last_name, t.teacher_id, '@school.edu.ph'), ' ', '')),
  t.contact_number,
  CONCAT(t.specialization, ', Davao City'),
  t.hire_date,
  1,
  CONCAT(t.hire_date, ' 08:00:00'),
  NULL
FROM teachers t
ON DUPLICATE KEY UPDATE
  FirstName = VALUES(FirstName),
  LastName = VALUES(LastName),
  Email = VALUES(Email),
  Phone = VALUES(Phone),
  Address = VALUES(Address),
  HireDate = VALUES(HireDate),
  IsActive = 1,
  UpdatedAt = UTC_TIMESTAMP();

-- Students from students
INSERT INTO student
(
  StudentNumber, FirstName, LastName, MiddleName, Gender, BirthDate, Email, Phone, Address, PhotoPath, IsActive, CreatedAt, UpdatedAt
)
SELECT
  CONCAT('STU-2026-', LPAD(1000 + s.student_id, 4, '0')),
  s.first_name,
  s.last_name,
  s.middle_name,
  s.gender,
  s.birthdate,
  LOWER(REPLACE(CONCAT(s.first_name, '.', s.last_name, s.student_id, '@gmail.com'), ' ', '')),
  s.contact_number,
  s.address,
  NULL,
  1,
  s.created_at,
  NULL
FROM students s
ON DUPLICATE KEY UPDATE
  FirstName = VALUES(FirstName),
  LastName = VALUES(LastName),
  MiddleName = VALUES(MiddleName),
  Gender = VALUES(Gender),
  BirthDate = VALUES(BirthDate),
  Email = VALUES(Email),
  Phone = VALUES(Phone),
  Address = VALUES(Address),
  IsActive = 1,
  UpdatedAt = UTC_TIMESTAMP();

-- Subjects from grades (distinct per course + subject_name)
INSERT INTO subject (SubjectCode, SubjectName, Units, CourseId, IsActive, CreatedAt, UpdatedAt)
SELECT
  CONCAT(UPPER(LEFT(c.CourseCode, 4)), '-', UPPER(SUBSTRING(MD5(CONCAT(c.CourseId, '|', g.subject_name)), 1, 6))) AS SubjectCode,
  g.subject_name,
  3,
  c.CourseId,
  1,
  UTC_TIMESTAMP(),
  NULL
FROM grades g
INNER JOIN enrollments e ON e.enrollment_id = g.enrollment_id
INNER JOIN sections sx ON sx.section_id = e.section_id
INNER JOIN course c ON c.CourseCode = CASE sx.course_id
    WHEN 1 THEN 'BSIT'
    WHEN 2 THEN 'BSED'
    WHEN 3 THEN 'BSBA'
    WHEN 4 THEN 'BSCRIM'
    WHEN 5 THEN 'BSN'
    ELSE 'BSIT'
END
GROUP BY c.CourseId, g.subject_name
ON DUPLICATE KEY UPDATE
  SubjectName = VALUES(SubjectName),
  Units = VALUES(Units),
  CourseId = VALUES(CourseId),
  IsActive = 1,
  UpdatedAt = UTC_TIMESTAMP();

-- Enrollments from enrollments
INSERT INTO enrollment
(
  EnrollmentNumber, StudentId, CourseId, AcademicYearId, YearLevelId, SemesterId, SectionId, EnrollDate, TotalUnits, Status, CreatedAt, UpdatedAt
)
SELECT
  CONCAT('ENR-2026-', LPAD(1000 + e.enrollment_id, 4, '0')) AS EnrollmentNumber,
  st.StudentId,
  c.CourseId,
  @ayId,
  sec.YearLevelId,
  @semId,
  sec.SectionId,
  e.enrollment_date,
  0,
  e.status,
  UTC_TIMESTAMP(),
  NULL
FROM enrollments e
INNER JOIN sections sx ON sx.section_id = e.section_id
INNER JOIN course c ON c.CourseCode = CASE sx.course_id
    WHEN 1 THEN 'BSIT'
    WHEN 2 THEN 'BSED'
    WHEN 3 THEN 'BSBA'
    WHEN 4 THEN 'BSCRIM'
    WHEN 5 THEN 'BSN'
    ELSE 'BSIT'
END
INNER JOIN section sec ON sec.SectionName = sx.section_name AND sec.CourseId = c.CourseId AND sec.AcademicYearId = @ayId AND sec.SemesterId = @semId
INNER JOIN student st ON st.StudentNumber = CONCAT('STU-2026-', LPAD(1000 + e.student_id, 4, '0'))
ON DUPLICATE KEY UPDATE
  StudentId = VALUES(StudentId),
  CourseId = VALUES(CourseId),
  AcademicYearId = VALUES(AcademicYearId),
  YearLevelId = VALUES(YearLevelId),
  SemesterId = VALUES(SemesterId),
  SectionId = VALUES(SectionId),
  EnrollDate = VALUES(EnrollDate),
  Status = VALUES(Status),
  UpdatedAt = UTC_TIMESTAMP();

-- Enrollment details + grades
INSERT INTO enrollmentdetails
(
  EnrollmentId, SubjectId, Units, ClassScheduleId, Grade, CreatedAt
)
SELECT
  en.EnrollmentId,
  sub.SubjectId,
  3,
  NULL,
  g.grade_value,
  UTC_TIMESTAMP()
FROM grades g
INNER JOIN enrollments e ON e.enrollment_id = g.enrollment_id
INNER JOIN sections sx ON sx.section_id = e.section_id
INNER JOIN course c ON c.CourseCode = CASE sx.course_id
    WHEN 1 THEN 'BSIT'
    WHEN 2 THEN 'BSED'
    WHEN 3 THEN 'BSBA'
    WHEN 4 THEN 'BSCRIM'
    WHEN 5 THEN 'BSN'
    ELSE 'BSIT'
END
INNER JOIN enrollment en ON en.EnrollmentNumber = CONCAT('ENR-2026-', LPAD(1000 + e.enrollment_id, 4, '0'))
INNER JOIN subject sub ON sub.CourseId = c.CourseId AND sub.SubjectName = g.subject_name
WHERE NOT EXISTS
(
  SELECT 1
  FROM enrollmentdetails ed
  WHERE ed.EnrollmentId = en.EnrollmentId
    AND ed.SubjectId = sub.SubjectId
);

-- Recompute total units
UPDATE enrollment e
SET e.TotalUnits = (
  SELECT IFNULL(SUM(ed.Units), 0)
  FROM enrollmentdetails ed
  WHERE ed.EnrollmentId = e.EnrollmentId
),
e.UpdatedAt = UTC_TIMESTAMP();
