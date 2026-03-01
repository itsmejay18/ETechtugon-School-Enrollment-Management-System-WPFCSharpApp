CREATE TABLE IF NOT EXISTS courses (
  course_id INT PRIMARY KEY,
  course_name VARCHAR(150) NOT NULL,
  department VARCHAR(150) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS sections (
  section_id INT PRIMARY KEY,
  course_id INT NOT NULL,
  section_name VARCHAR(80) NOT NULL,
  school_year VARCHAR(20) NOT NULL,
  CONSTRAINT fk_sections_course FOREIGN KEY (course_id) REFERENCES courses(course_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS teachers (
  teacher_id INT PRIMARY KEY,
  first_name VARCHAR(80) NOT NULL,
  last_name VARCHAR(80) NOT NULL,
  specialization VARCHAR(120) NOT NULL,
  contact_number VARCHAR(11) NOT NULL,
  hire_date DATE NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS students (
  student_id INT PRIMARY KEY,
  first_name VARCHAR(80) NOT NULL,
  last_name VARCHAR(80) NOT NULL,
  middle_name VARCHAR(80) NULL,
  birthdate DATE NOT NULL,
  gender VARCHAR(10) NOT NULL,
  address VARCHAR(255) NOT NULL,
  contact_number VARCHAR(11) NOT NULL,
  created_at DATETIME NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS enrollments (
  enrollment_id INT PRIMARY KEY,
  student_id INT NOT NULL,
  section_id INT NOT NULL,
  enrollment_date DATE NOT NULL,
  status VARCHAR(20) NOT NULL,
  CONSTRAINT fk_enrollments_student FOREIGN KEY (student_id) REFERENCES students(student_id),
  CONSTRAINT fk_enrollments_section FOREIGN KEY (section_id) REFERENCES sections(section_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS grades (
  grade_id INT PRIMARY KEY,
  enrollment_id INT NOT NULL,
  subject_name VARCHAR(150) NOT NULL,
  grade_value DECIMAL(2,1) NOT NULL,
  CONSTRAINT fk_grades_enrollment FOREIGN KEY (enrollment_id) REFERENCES enrollments(enrollment_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
