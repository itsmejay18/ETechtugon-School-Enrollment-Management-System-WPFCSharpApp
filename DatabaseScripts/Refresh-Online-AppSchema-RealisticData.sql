-- Replace placeholder/generated rows with realistic sample data.
-- Uses the existing 30-row app-schema dataset and keeps IDs stable.

START TRANSACTION;

CREATE TEMPORARY TABLE seed_department (
    DepartmentId INT PRIMARY KEY,
    DepartmentCode VARCHAR(20) NOT NULL,
    DepartmentName VARCHAR(100) NOT NULL,
    Description VARCHAR(250) NULL
);

INSERT INTO seed_department (DepartmentId, DepartmentCode, DepartmentName, Description) VALUES
(1, 'CS', 'Department of Computer Science', 'Programming, algorithms, and software development.'),
(2, 'IT', 'Department of Information Technology', 'Infrastructure, web technologies, and systems support.'),
(3, 'IS', 'Department of Information Systems', 'Business systems analysis, ERP, and digital transformation.'),
(4, 'CYB', 'Department of Cybersecurity', 'Network defense, risk management, and security operations.'),
(5, 'DA', 'Department of Data Analytics', 'Applied analytics, visualization, and data-driven decision making.'),
(6, 'BA', 'Department of Business Administration', 'Management, marketing, and entrepreneurship.'),
(7, 'ACC', 'Department of Accountancy', 'Financial reporting, auditing, and taxation.'),
(8, 'ECO', 'Department of Economics', 'Economic policy, development, and market analysis.'),
(9, 'EDU', 'Department of Teacher Education', 'Pedagogy, curriculum planning, and classroom practice.'),
(10, 'PSY', 'Department of Psychology', 'Behavioral science, counseling, and human development.'),
(11, 'NUR', 'Department of Nursing', 'Community health, patient care, and clinical practice.'),
(12, 'PHA', 'Department of Pharmacy', 'Pharmaceutical care, compounding, and drug information.'),
(13, 'CRM', 'Department of Criminology', 'Criminal justice, law enforcement, and forensic practice.'),
(14, 'CVE', 'Department of Civil Engineering', 'Construction systems, structures, and transport planning.'),
(15, 'MEE', 'Department of Mechanical Engineering', 'Machine design, manufacturing, and thermal systems.'),
(16, 'EEE', 'Department of Electrical Engineering', 'Power systems, controls, and electrical design.'),
(17, 'ECE', 'Department of Electronics Engineering', 'Electronics, communication systems, and embedded devices.'),
(18, 'ARC', 'Department of Architecture', 'Design studio, planning, and built environment studies.'),
(19, 'AGR', 'Department of Agriculture', 'Crop production, soil management, and farm technology.'),
(20, 'FMS', 'Department of Fisheries and Marine Studies', 'Aquaculture, coastal resources, and fish processing.'),
(21, 'HM', 'Department of Hospitality Management', 'Hotel operations, food service, and guest experience.'),
(22, 'TM', 'Department of Tourism Management', 'Travel operations, destination planning, and tour services.'),
(23, 'ENG', 'Department of English Studies', 'Language, writing, and communication studies.'),
(24, 'POL', 'Department of Political Science', 'Government systems, policy, and civic leadership.'),
(25, 'PAD', 'Department of Public Administration', 'Public service management, local governance, and planning.'),
(26, 'MAT', 'Department of Mathematics', 'Pure and applied mathematics for science and industry.'),
(27, 'BIO', 'Department of Biology', 'Life sciences, ecology, and laboratory research.'),
(28, 'CHE', 'Department of Chemistry', 'Chemical analysis, laboratory methods, and materials.'),
(29, 'PHY', 'Department of Physics', 'Classical and modern physics for scientific applications.'),
(30, 'SWK', 'Department of Social Work', 'Community engagement, welfare practice, and case management.');

UPDATE department d
INNER JOIN seed_department s ON s.DepartmentId = d.DepartmentId
SET d.DepartmentCode = s.DepartmentCode,
    d.DepartmentName = s.DepartmentName,
    d.Description = s.Description,
    d.IsActive = 1,
    d.UpdatedAt = UTC_TIMESTAMP();

CREATE TEMPORARY TABLE seed_course (
    CourseId INT PRIMARY KEY,
    CourseCode VARCHAR(20) NOT NULL,
    CourseName VARCHAR(100) NOT NULL,
    Description VARCHAR(250) NULL,
    DepartmentId INT NOT NULL
);

INSERT INTO seed_course (CourseId, CourseCode, CourseName, Description, DepartmentId) VALUES
(1, 'BSCS', 'Bachelor of Science in Computer Science', 'Core computing, software engineering, and algorithmic problem solving.', 1),
(2, 'BSIT', 'Bachelor of Science in Information Technology', 'Applications development, systems support, and web technologies.', 2),
(3, 'BSIS', 'Bachelor of Science in Information Systems', 'Enterprise systems, business process analysis, and digital operations.', 3),
(4, 'BSCY', 'Bachelor of Science in Cybersecurity', 'Threat detection, security governance, and digital risk management.', 4),
(5, 'BSDA', 'Bachelor of Science in Data Analytics', 'Data modeling, dashboarding, and business intelligence.', 5),
(6, 'BSBA', 'Bachelor of Science in Business Administration', 'Management, operations, and strategic planning.', 6),
(7, 'BSA', 'Bachelor of Science in Accountancy', 'Financial accounting, auditing, and tax compliance.', 7),
(8, 'BSECON', 'Bachelor of Science in Economics', 'Economic theory, development policy, and quantitative analysis.', 8),
(9, 'BSED', 'Bachelor of Secondary Education', 'Teaching methods, learner development, and curriculum design.', 9),
(10, 'BSPSY', 'Bachelor of Science in Psychology', 'Behavioral science, testing, and counseling foundations.', 10),
(11, 'BSN', 'Bachelor of Science in Nursing', 'Health assessment, nursing process, and clinical care.', 11),
(12, 'BSPHARM', 'Bachelor of Science in Pharmacy', 'Drug therapy, pharmaceutical science, and patient counseling.', 12),
(13, 'BSCRIM', 'Bachelor of Science in Criminology', 'Law enforcement systems, criminal law, and investigation.', 13),
(14, 'BSCE', 'Bachelor of Science in Civil Engineering', 'Structures, transport systems, and site development.', 14),
(15, 'BSME', 'Bachelor of Science in Mechanical Engineering', 'Mechanical systems, thermal sciences, and design applications.', 15),
(16, 'BSEE', 'Bachelor of Science in Electrical Engineering', 'Electrical systems, power distribution, and circuit design.', 16),
(17, 'BSECE', 'Bachelor of Science in Electronics Engineering', 'Communication, control systems, and electronic design.', 17),
(18, 'BSARCH', 'Bachelor of Science in Architecture', 'Design studio, environmental planning, and construction documents.', 18),
(19, 'BSAGRI', 'Bachelor of Science in Agriculture', 'Crop science, farm management, and production systems.', 19),
(20, 'BSFISH', 'Bachelor of Science in Fisheries', 'Aquaculture operations, fishery resources, and processing.', 20),
(21, 'BSHM', 'Bachelor of Science in Hospitality Management', 'Hotel administration, service quality, and guest relations.', 21),
(22, 'BSTM', 'Bachelor of Science in Tourism Management', 'Destination management, travel services, and tourism planning.', 22),
(23, 'ABELS', 'Bachelor of Arts in English Language Studies', 'Writing, discourse analysis, and professional communication.', 23),
(24, 'ABPOL', 'Bachelor of Arts in Political Science', 'Politics, public affairs, and democratic institutions.', 24),
(25, 'BSPA', 'Bachelor of Science in Public Administration', 'Public sector leadership, budgeting, and local governance.', 25),
(26, 'BSMATH', 'Bachelor of Science in Mathematics', 'Advanced mathematics, modeling, and analytical reasoning.', 26),
(27, 'BSBIO', 'Bachelor of Science in Biology', 'Cell biology, ecology, and scientific investigation.', 27),
(28, 'BSCHEM', 'Bachelor of Science in Chemistry', 'Analytical methods, laboratory practice, and chemical systems.', 28),
(29, 'BSPHYS', 'Bachelor of Science in Physics', 'Physical principles, instrumentation, and applied computation.', 29),
(30, 'BSSW', 'Bachelor of Science in Social Work', 'Community work, social welfare systems, and intervention practice.', 30);

UPDATE course c
INNER JOIN seed_course s ON s.CourseId = c.CourseId
SET c.CourseCode = s.CourseCode,
    c.CourseName = s.CourseName,
    c.Description = s.Description,
    c.DepartmentId = s.DepartmentId,
    c.IsActive = 1,
    c.UpdatedAt = UTC_TIMESTAMP();

CREATE TEMPORARY TABLE seed_subject (
    SubjectId INT PRIMARY KEY,
    SubjectCode VARCHAR(20) NOT NULL,
    SubjectName VARCHAR(100) NOT NULL,
    Units INT NOT NULL,
    CourseId INT NOT NULL
);

INSERT INTO seed_subject (SubjectId, SubjectCode, SubjectName, Units, CourseId) VALUES
(1, 'CS201', 'Object-Oriented Programming', 3, 1),
(2, 'IT204', 'Web Systems and Technologies', 3, 2),
(3, 'IS205', 'Systems Analysis and Design', 3, 3),
(4, 'CY201', 'Security Operations Fundamentals', 3, 4),
(5, 'DA202', 'Applied Data Visualization', 3, 5),
(6, 'BA201', 'Fundamentals of Management', 3, 6),
(7, 'AC203', 'Intermediate Financial Accounting', 3, 7),
(8, 'EC201', 'Development Economics', 3, 8),
(9, 'ED204', 'Principles of Teaching I', 3, 9),
(10, 'PSY205', 'Abnormal Psychology', 3, 10),
(11, 'NUR206', 'Medical-Surgical Nursing I', 4, 11),
(12, 'PHA202', 'Pharmacology Fundamentals', 4, 12),
(13, 'CRM201', 'Criminal Law and Procedure', 3, 13),
(14, 'CE202', 'Structural Theory I', 3, 14),
(15, 'ME201', 'Engineering Materials', 3, 15),
(16, 'EE203', 'Power Systems Analysis', 3, 16),
(17, 'ECE204', 'Digital Electronics', 3, 17),
(18, 'AR205', 'Architectural Design Studio I', 4, 18),
(19, 'AG201', 'Soil Science and Crop Production', 3, 19),
(20, 'FS202', 'Aquaculture Production Systems', 3, 20),
(21, 'HM203', 'Food and Beverage Service Operations', 3, 21),
(22, 'TM204', 'Tourism Planning and Sustainable Development', 3, 22),
(23, 'EN201', 'Academic Writing and Stylistics', 3, 23),
(24, 'POL202', 'Comparative Politics', 3, 24),
(25, 'PA203', 'Local Governance and Public Finance', 3, 25),
(26, 'MAT201', 'Calculus and Linear Algebra', 3, 26),
(27, 'BIO204', 'Genetics and Evolution', 3, 27),
(28, 'CHE205', 'Analytical Chemistry', 4, 28),
(29, 'PHY202', 'Electricity and Magnetism', 3, 29),
(30, 'SW201', 'Human Behavior and Social Environment', 3, 30);

UPDATE subject s
INNER JOIN seed_subject x ON x.SubjectId = s.SubjectId
SET s.SubjectCode = x.SubjectCode,
    s.SubjectName = x.SubjectName,
    s.Units = x.Units,
    s.CourseId = x.CourseId,
    s.IsActive = 1,
    s.UpdatedAt = UTC_TIMESTAMP();

CREATE TEMPORARY TABLE seed_faculty (
    FacultyId INT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50) NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(30) NOT NULL,
    Address VARCHAR(250) NOT NULL,
    HireDate DATE NOT NULL
);

INSERT INTO seed_faculty (FacultyId, FirstName, LastName, MiddleName, Email, Phone, Address, HireDate) VALUES
(1, 'Maria Luisa', 'Santos', 'Lopez', 'maria.santos@school.edu.ph', '09171234001', 'Poblacion, Digos City, Davao del Sur', '2018-06-04'),
(2, 'John Michael', 'Cruz', 'Reyes', 'john.cruz@school.edu.ph', '09171234002', 'Toril, Davao City', '2017-06-05'),
(3, 'Angela Marie', 'Dela Cruz', 'Torres', 'angela.delacruz@school.edu.ph', '09171234003', 'Tagum City, Davao del Norte', '2019-06-06'),
(4, 'Richard', 'Mendoza', 'Ramos', 'richard.mendoza@school.edu.ph', '09171234004', 'Panabo City, Davao del Norte', '2020-06-07'),
(5, 'Karen Joy', 'Villanueva', 'Santos', 'karen.villanueva@school.edu.ph', '09171234005', 'Mintal, Davao City', '2018-06-10'),
(6, 'Allan', 'Bautista', 'Mercado', 'allan.bautista@school.edu.ph', '09171234006', 'Matti, Digos City, Davao del Sur', '2016-06-13'),
(7, 'Grace Anne', 'Ramos', 'Garcia', 'grace.ramos@school.edu.ph', '09171234007', 'Sta. Cruz, Davao del Sur', '2019-06-17'),
(8, 'Eric', 'Navarro', 'Flores', 'eric.navarro@school.edu.ph', '09171234008', 'Bansalan, Davao del Sur', '2017-06-20'),
(9, 'Joanna', 'Flores', 'Castillo', 'joanna.flores@school.edu.ph', '09171234009', 'Sulop, Davao del Sur', '2021-06-21'),
(10, 'Paolo', 'Garcia', 'Aquino', 'paolo.garcia@school.edu.ph', '09171234010', 'Matina, Davao City', '2018-06-24'),
(11, 'Liza', 'Reyes', 'Fernandez', 'liza.reyes@school.edu.ph', '09171234011', 'Bangkal, Davao City', '2015-06-26'),
(12, 'Mark Anthony', 'Torres', 'Domingo', 'mark.torres@school.edu.ph', '09171234012', 'Bajada, Davao City', '2017-07-01'),
(13, 'Hazel', 'Domingo', 'Cabrera', 'hazel.domingo@school.edu.ph', '09171234013', 'Lanang, Davao City', '2019-07-03'),
(14, 'Noel', 'Castillo', 'Marquez', 'noel.castillo@school.edu.ph', '09171234014', 'Catalunan Pequenio, Davao City', '2016-07-08'),
(15, 'Irene', 'Aquino', 'Gonzales', 'irene.aquino@school.edu.ph', '09171234015', 'Calinan, Davao City', '2020-07-09'),
(16, 'Dennis', 'Salazar', 'Manalo', 'dennis.salazar@school.edu.ph', '09171234016', 'Tugbok, Davao City', '2018-07-10'),
(17, 'Camille', 'Fernandez', 'David', 'camille.fernandez@school.edu.ph', '09171234017', 'Padada, Davao del Sur', '2019-07-15'),
(18, 'Roberto', 'Mercado', 'Estrella', 'roberto.mercado@school.edu.ph', '09171234018', 'Hagonoy, Davao del Sur', '2017-07-17'),
(19, 'Janine', 'Soriano', 'Alonzo', 'janine.soriano@school.edu.ph', '09171234019', 'Malalag, Davao del Sur', '2021-07-18'),
(20, 'Victor', 'Lim', 'Sebastian', 'victor.lim@school.edu.ph', '09171234020', 'Matanao, Davao del Sur', '2016-07-22'),
(21, 'Sheila', 'Robles', 'Padilla', 'sheila.robles@school.edu.ph', '09171234021', 'Kiblawan, Davao del Sur', '2018-07-24'),
(22, 'Adrian', 'Dela Pena', 'Bautista', 'adrian.delapena@school.edu.ph', '09171234022', 'Santa Maria, Davao Occidental', '2019-07-29'),
(23, 'Ruth', 'Cabrera', 'Ventura', 'ruth.cabrera@school.edu.ph', '09171234023', 'Don Marcelino, Davao Occidental', '2017-07-31'),
(24, 'Leo', 'Marquez', 'Rivas', 'leo.marquez@school.edu.ph', '09171234024', 'Malita, Davao Occidental', '2015-08-05'),
(25, 'Faith', 'Gonzales', 'Ortega', 'faith.gonzales@school.edu.ph', '09171234025', 'Jose Abad Santos, Davao Occidental', '2020-08-07'),
(26, 'Carlo', 'Manalo', 'Gutierrez', 'carlo.manalo@school.edu.ph', '09171234026', 'Magsaysay, Davao del Sur', '2018-08-12'),
(27, 'Kristine', 'David', 'Ramirez', 'kristine.david@school.edu.ph', '09171234027', 'Digos Heights, Digos City', '2019-08-14'),
(28, 'Ramon', 'Estrella', 'Mendoza', 'ramon.estrella@school.edu.ph', '09171234028', 'Mintal Proper, Davao City', '2016-08-19'),
(29, 'Patricia', 'Alonzo', 'Navarro', 'patricia.alonzo@school.edu.ph', '09171234029', 'Panacan, Davao City', '2017-08-21'),
(30, 'Miguel', 'Sebastian', 'Flores', 'miguel.sebastian@school.edu.ph', '09171234030', 'Buhangin, Davao City', '2021-08-26');

UPDATE faculty f
INNER JOIN seed_faculty s ON s.FacultyId = f.FacultyId
SET f.FacultyCode = CONCAT('FAC-2026-', LPAD(f.FacultyId, 4, '0')),
    f.FirstName = s.FirstName,
    f.LastName = s.LastName,
    f.MiddleName = s.MiddleName,
    f.Email = s.Email,
    f.Phone = s.Phone,
    f.Address = s.Address,
    f.PhotoPath = NULL,
    f.HireDate = s.HireDate,
    f.IsActive = 1,
    f.UpdatedAt = UTC_TIMESTAMP();

CREATE TEMPORARY TABLE seed_student (
    StudentId INT PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    MiddleName VARCHAR(50) NULL,
    Gender VARCHAR(20) NOT NULL,
    BirthDate DATE NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Phone VARCHAR(30) NOT NULL,
    Address VARCHAR(250) NOT NULL
);

INSERT INTO seed_student (StudentId, FirstName, LastName, MiddleName, Gender, BirthDate, Email, Phone, Address) VALUES
(1, 'Jay', 'Ababon', 'Jemino', 'Male', '2004-06-18', 'jay.ababon@student.school.edu.ph', '09912268122', 'Lower Balutakay, Hagonoy, Davao del Sur'),
(2, 'Alyssa Mae', 'Gutierrez', 'Lopez', 'Female', '2005-01-16', 'alyssa.gutierrez@student.school.edu.ph', '09910000002', 'Poblacion, Digos City, Davao del Sur'),
(3, 'John Carlo', 'Ramirez', 'Santos', 'Male', '2004-09-24', 'john.ramirez@student.school.edu.ph', '09910000003', 'Toril, Davao City'),
(4, 'Princess Anne', 'Mendoza', 'Reyes', 'Female', '2005-02-11', 'princess.mendoza@student.school.edu.ph', '09910000004', 'Tagum City, Davao del Norte'),
(5, 'Christian Dale', 'Navarro', 'Garcia', 'Male', '2004-07-08', 'christian.navarro@student.school.edu.ph', '09910000005', 'Panabo City, Davao del Norte'),
(6, 'Hannah Grace', 'Flores', 'Aquino', 'Female', '2005-03-20', 'hannah.flores@student.school.edu.ph', '09910000006', 'Mintal, Davao City'),
(7, 'Mark Joseph', 'Reyes', 'Fernandez', 'Male', '2004-10-30', 'mark.reyes@student.school.edu.ph', '09910000007', 'Matti, Digos City, Davao del Sur'),
(8, 'Angelica Marie', 'Torres', 'Domingo', 'Female', '2005-05-12', 'angelica.torres@student.school.edu.ph', '09910000008', 'Sta. Cruz, Davao del Sur'),
(9, 'Kevin Paul', 'Castillo', 'Cabrera', 'Male', '2004-04-09', 'kevin.castillo@student.school.edu.ph', '09910000009', 'Bansalan, Davao del Sur'),
(10, 'Rose Ann', 'Aquino', 'Marquez', 'Female', '2005-06-03', 'rose.aquino@student.school.edu.ph', '09910000010', 'Sulop, Davao del Sur'),
(11, 'Bryan James', 'Salazar', 'Gonzales', 'Male', '2004-11-27', 'bryan.salazar@student.school.edu.ph', '09910000011', 'Matina, Davao City'),
(12, 'Faith Nicole', 'Fernandez', 'David', 'Female', '2005-08-18', 'faith.fernandez@student.school.edu.ph', '09910000012', 'Bangkal, Davao City'),
(13, 'Joshua Daniel', 'Mercado', 'Estrella', 'Male', '2004-12-14', 'joshua.mercado@student.school.edu.ph', '09910000013', 'Bajada, Davao City'),
(14, 'Samantha Joy', 'Soriano', 'Alonzo', 'Female', '2005-07-05', 'samantha.soriano@student.school.edu.ph', '09910000014', 'Lanang, Davao City'),
(15, 'Vincent Paul', 'Lim', 'Sebastian', 'Male', '2004-03-01', 'vincent.lim@student.school.edu.ph', '09910000015', 'Catalunan Pequenio, Davao City'),
(16, 'Christine Mae', 'Robles', 'Padilla', 'Female', '2005-09-09', 'christine.robles@student.school.edu.ph', '09910000016', 'Calinan, Davao City'),
(17, 'Aaron Kyle', 'Cabrera', 'Bautista', 'Male', '2004-01-29', 'aaron.cabrera@student.school.edu.ph', '09910000017', 'Tugbok, Davao City'),
(18, 'Bea Denise', 'Marquez', 'Ventura', 'Female', '2005-10-21', 'bea.marquez@student.school.edu.ph', '09910000018', 'Padada, Davao del Sur'),
(19, 'Nathaniel', 'Gonzales', 'Rivas', 'Male', '2004-02-14', 'nathaniel.gonzales@student.school.edu.ph', '09910000019', 'Hagonoy, Davao del Sur'),
(20, 'Jessa Mae', 'Manalo', 'Ortega', 'Female', '2005-11-02', 'jessa.manalo@student.school.edu.ph', '09910000020', 'Malalag, Davao del Sur'),
(21, 'Kurt Adrian', 'David', 'Gutierrez', 'Male', '2004-05-17', 'kurt.david@student.school.edu.ph', '09910000021', 'Matanao, Davao del Sur'),
(22, 'Shaina', 'Estrella', 'Ramirez', 'Female', '2005-04-25', 'shaina.estrella@student.school.edu.ph', '09910000022', 'Kiblawan, Davao del Sur'),
(23, 'Angelo Paolo', 'Alonzo', 'Mendoza', 'Male', '2004-08-13', 'angelo.alonzo@student.school.edu.ph', '09910000023', 'Santa Maria, Davao Occidental'),
(24, 'Camille Rose', 'Sebastian', 'Navarro', 'Female', '2005-12-07', 'camille.sebastian@student.school.edu.ph', '09910000024', 'Don Marcelino, Davao Occidental'),
(25, 'Ivan Patrick', 'De Leon', 'Flores', 'Male', '2004-06-27', 'ivan.deleon@student.school.edu.ph', '09910000025', 'Malita, Davao Occidental'),
(26, 'Joy Marie', 'Padilla', 'Mercado', 'Female', '2005-01-04', 'joy.padilla@student.school.edu.ph', '09910000026', 'Jose Abad Santos, Davao Occidental'),
(27, 'Neil Anthony', 'Bautista', 'Soriano', 'Male', '2004-09-10', 'neil.bautista@student.school.edu.ph', '09910000027', 'Magsaysay, Davao del Sur'),
(28, 'Trisha Mae', 'Ortega', 'Lim', 'Female', '2005-03-29', 'trisha.ortega@student.school.edu.ph', '09910000028', 'Digos Heights, Digos City'),
(29, 'Francis Miguel', 'Ventura', 'Robles', 'Male', '2004-07-22', 'francis.ventura@student.school.edu.ph', '09910000029', 'Panacan, Davao City'),
(30, 'Claire Louise', 'Rivas', 'Cabrera', 'Female', '2005-02-15', 'claire.rivas@student.school.edu.ph', '09910000030', 'Buhangin, Davao City');

UPDATE student s
INNER JOIN seed_student x ON x.StudentId = s.StudentId
SET s.StudentNumber = CONCAT('STU-2026-', LPAD(s.StudentId, 4, '0')),
    s.FirstName = x.FirstName,
    s.LastName = x.LastName,
    s.MiddleName = x.MiddleName,
    s.Gender = x.Gender,
    s.BirthDate = x.BirthDate,
    s.Email = x.Email,
    s.Phone = x.Phone,
    s.Address = x.Address,
    s.PhotoPath = NULL,
    s.IsActive = 1,
    s.UpdatedAt = UTC_TIMESTAMP();

CREATE TEMPORARY TABLE seed_user (
    UserId INT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL,
    RoleName VARCHAR(30) NOT NULL,
    DisplayName VARCHAR(100) NOT NULL,
    IsActive TINYINT(1) NOT NULL
);

INSERT INTO seed_user (UserId, Username, RoleName, DisplayName, IsActive) VALUES
(1, 'admin', 'Admin', 'System Administrator', 1),
(2, 'registrar', 'Registrar', 'Registrar Office', 1),
(3, 'faculty1', 'Faculty', 'Prof. Maria Luisa Santos', 1),
(4, 'dean.it', 'Admin', 'Dean Angela Marie Dela Cruz', 0),
(5, 'dean.analytics', 'Admin', 'Dean Richard Mendoza', 0),
(6, 'dean.business', 'Admin', 'Dean Allan Bautista', 0),
(7, 'dean.education', 'Admin', 'Dean Grace Ramos', 0),
(8, 'registrar.navarro', 'Registrar', 'Registrar Eric Navarro', 0),
(9, 'records.flores', 'Registrar', 'Records Officer Joanna Flores', 0),
(10, 'faculty.cruz', 'Faculty', 'Prof. John Michael Cruz', 0),
(11, 'faculty.reyes', 'Faculty', 'Prof. Liza Reyes', 0),
(12, 'faculty.torres', 'Faculty', 'Prof. Mark Anthony Torres', 0),
(13, 'faculty.domingo', 'Faculty', 'Prof. Hazel Domingo', 0),
(14, 'faculty.castillo', 'Faculty', 'Prof. Noel Castillo', 0),
(15, 'faculty.aquino', 'Faculty', 'Prof. Irene Aquino', 0),
(16, 'faculty.salazar', 'Faculty', 'Prof. Dennis Salazar', 0),
(17, 'faculty.fernandez', 'Faculty', 'Prof. Camille Fernandez', 0),
(18, 'faculty.mercado', 'Faculty', 'Prof. Roberto Mercado', 0),
(19, 'faculty.soriano', 'Faculty', 'Prof. Janine Soriano', 0),
(20, 'faculty.lim', 'Faculty', 'Prof. Victor Lim', 0),
(21, 'faculty.robles', 'Faculty', 'Prof. Sheila Robles', 0),
(22, 'faculty.delapena', 'Faculty', 'Prof. Adrian Dela Pena', 0),
(23, 'admissions.cabrera', 'Registrar', 'Admissions Officer Ruth Cabrera', 0),
(24, 'guidance.marquez', 'Registrar', 'Guidance Officer Leo Marquez', 0),
(25, 'extension.gonzales', 'Registrar', 'Extension Officer Faith Gonzales', 0),
(26, 'faculty.manalo', 'Faculty', 'Prof. Carlo Manalo', 0),
(27, 'faculty.david', 'Faculty', 'Prof. Kristine David', 0),
(28, 'faculty.estrella', 'Faculty', 'Prof. Ramon Estrella', 0),
(29, 'faculty.alonzo', 'Faculty', 'Prof. Patricia Alonzo', 0),
(30, 'faculty.sebastian', 'Faculty', 'Prof. Miguel Sebastian', 0);

UPDATE users u
INNER JOIN seed_user s ON s.UserId = u.UserId
SET u.Username = s.Username,
    u.Role = s.RoleName,
    u.DisplayName = s.DisplayName,
    u.IsActive = s.IsActive,
    u.UpdatedAt = UTC_TIMESTAMP()
WHERE u.UserId BETWEEN 1 AND 30;

UPDATE section s
INNER JOIN course c ON c.CourseId = s.SectionId
SET s.SectionName = CONCAT(c.CourseCode, '-', ((s.SectionId - 1) MOD 4) + 1, 'A'),
    s.CourseId = s.SectionId,
    s.YearLevelId = ((s.SectionId - 1) MOD 4) + 1,
    s.AcademicYearId = 1,
    s.SemesterId = 1,
    s.Capacity = 40 + MOD(s.SectionId, 6) * 5,
    s.IsActive = 1,
    s.UpdatedAt = UTC_TIMESTAMP()
WHERE s.SectionId BETWEEN 1 AND 30;

UPDATE curriculum cur
INNER JOIN course c ON c.CourseId = cur.CurriculumId
INNER JOIN yearlevel yl ON yl.YearLevelId = ((cur.CurriculumId - 1) MOD 4) + 1
INNER JOIN semester sem ON sem.SemesterId = 1
INNER JOIN academicyear ay ON ay.AcademicYearId = 1
SET cur.CourseId = cur.CurriculumId,
    cur.YearLevelId = ((cur.CurriculumId - 1) MOD 4) + 1,
    cur.SemesterId = 1,
    cur.AcademicYearId = 1,
    cur.Name = CONCAT(c.CourseCode, ' - ', c.CourseName, ' - ', yl.Name, ' - ', sem.Name, ' - ', ay.Name),
    cur.IsActive = 1,
    cur.UpdatedAt = UTC_TIMESTAMP()
WHERE cur.CurriculumId BETWEEN 1 AND 30;

UPDATE curriculumdetails
SET CurriculumId = CurriculumDetailId,
    SubjectId = CurriculumDetailId
WHERE CurriculumDetailId BETWEEN 1 AND 30;

UPDATE classschedule
SET SectionId = ClassScheduleId,
    SubjectId = ClassScheduleId,
    FacultyId = ClassScheduleId,
    DayOfWeek = ELT(((ClassScheduleId - 1) MOD 5) + 1, 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'),
    StartTime = MAKETIME(7 + ((ClassScheduleId - 1) MOD 5), 30, 0),
    EndTime = MAKETIME(9 + ((ClassScheduleId - 1) MOD 5), 0, 0),
    Room = CONCAT('Room ', 200 + ClassScheduleId),
    Remarks = 'Regular face-to-face schedule',
    AcademicYearId = 1,
    SemesterId = 1,
    IsActive = 1,
    UpdatedAt = UTC_TIMESTAMP()
WHERE ClassScheduleId BETWEEN 1 AND 30;

UPDATE enrollment
SET EnrollmentNumber = CONCAT('ENR-2026-', LPAD(EnrollmentId, 5, '0')),
    StudentId = EnrollmentId,
    CourseId = EnrollmentId,
    AcademicYearId = 1,
    YearLevelId = ((EnrollmentId - 1) MOD 4) + 1,
    SemesterId = 1,
    SectionId = EnrollmentId,
    EnrollDate = DATE_SUB('2026-06-16', INTERVAL EnrollmentId DAY),
    TotalUnits = 0,
    Status = 'Registered',
    UpdatedAt = UTC_TIMESTAMP()
WHERE EnrollmentId BETWEEN 1 AND 30;

UPDATE enrollmentdetails ed
INNER JOIN subject s ON s.SubjectId = ed.EnrollmentDetailId
SET ed.EnrollmentId = ed.EnrollmentDetailId,
    ed.SubjectId = ed.EnrollmentDetailId,
    ed.Units = s.Units,
    ed.ClassScheduleId = ed.EnrollmentDetailId,
    ed.Grade = ROUND(84 + MOD(ed.EnrollmentDetailId, 9) * 0.75, 2),
    ed.CreatedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL ed.EnrollmentDetailId DAY)
WHERE ed.EnrollmentDetailId BETWEEN 1 AND 30;

UPDATE enrollment e
SET e.TotalUnits = (
    SELECT IFNULL(SUM(ed.Units), 0)
    FROM enrollmentdetails ed
    WHERE ed.EnrollmentId = e.EnrollmentId
),
e.UpdatedAt = UTC_TIMESTAMP()
WHERE e.EnrollmentId BETWEEN 1 AND 30;

UPDATE activitylog
SET UserId = ((ActivityLogId - 1) MOD 30) + 1,
    Action = ELT(((ActivityLogId - 1) MOD 6) + 1, 'Login', 'Create', 'Update', 'Assign', 'Approve', 'Export'),
    Entity = ELT(((ActivityLogId - 1) MOD 6) + 1, 'User', 'Student', 'Course', 'ClassSchedule', 'Enrollment', 'Report'),
    EntityId = ActivityLogId,
    Details = ELT(((ActivityLogId - 1) MOD 6) + 1,
        'Authenticated to the online registrar portal.',
        'Created a student profile record.',
        'Updated a course or subject record.',
        'Assigned a faculty member to a class schedule.',
        'Approved a student enrollment transaction.',
        'Exported a registrar summary report.'
    ),
    MachineName = 'ONLINE-REGISTRY',
    CreatedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL ActivityLogId HOUR)
WHERE ActivityLogId BETWEEN 1 AND 30;

COMMIT;

SELECT d.DepartmentId, d.DepartmentCode, d.DepartmentName
FROM department d
ORDER BY d.DepartmentId
LIMIT 5;

SELECT c.CourseId, c.CourseCode, c.CourseName
FROM course c
ORDER BY c.CourseId
LIMIT 5;

SELECT f.FacultyId, f.FirstName, f.LastName, f.Email
FROM faculty f
ORDER BY f.FacultyId
LIMIT 5;

SELECT s.StudentId, s.FirstName, s.LastName, s.Email
FROM student s
ORDER BY s.StudentId
LIMIT 5;
