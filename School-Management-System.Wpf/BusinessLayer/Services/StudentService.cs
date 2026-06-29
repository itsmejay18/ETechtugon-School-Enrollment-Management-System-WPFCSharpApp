using System;
using System.Data;
using System.Text.RegularExpressions;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class StudentService
    {
        private readonly IStudentData _studentData;

        public StudentService(IStudentData studentData)
        {
            _studentData = studentData ?? throw new ArgumentNullException(nameof(studentData));
        }

        public DataTable GetStudents(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _studentData.GetAllActive();
            }

            return _studentData.Search(searchText);
        }

        public DataTable GetStudents(string searchText, string studentType)
        {
            return _studentData.Search(searchText, studentType);
        }

        public string GetNextStudentNumber()
        {
            return _studentData.GetNextStudentNumber();
        }

        public int GetActiveCount()
        {
            return _studentData.GetActiveCount();
        }

        public DataTable GetProfileSubjects(int studentId, int? academicYearId, int? semesterId)
        {
            return _studentData.GetEnrolledSubjects(studentId, academicYearId, semesterId);
        }

        public DataTable GetEnrollmentHistory(int studentId)
        {
            return _studentData.GetEnrollmentHistory(studentId);
        }

        public DataTable GetCompletedSubjects(int studentId)
        {
            return _studentData.GetCompletedSubjects(studentId);
        }

        public DataTable GetFailedSubjects(int studentId)
        {
            return _studentData.GetFailedSubjects(studentId);
        }

        public DataTable GetRemainingSubjects(int studentId, int? curriculumId)
        {
            return _studentData.GetRemainingSubjects(studentId, curriculumId);
        }

        public DataTable GetAcademicProfile(int studentId)
        {
            return _studentData.GetAcademicProfile(studentId);
        }

        public ValidationResult Validate(Student student)
        {
            var vr = new ValidationResult();
            if (student == null)
            {
                vr.Add("Student is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                vr.Add("Student Number is required.");
            }

            if (!IsValidStudentType(student.StudentType))
            {
                vr.Add("Student Type must be Regular, Irregular, or Summer.");
            }

            if (string.IsNullOrWhiteSpace(student.FirstName))
            {
                vr.Add("First Name is required.");
            }

            if (string.IsNullOrWhiteSpace(student.LastName))
            {
                vr.Add("Last Name is required.");
            }

            if (!string.IsNullOrWhiteSpace(student.Email))
            {
                // Basic email format check.
                if (!Regex.IsMatch(student.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    vr.Add("Email is invalid.");
                }
            }

            return vr;
        }

        public static string NormalizeStudentType(string value)
        {
            if (string.Equals(value, "Irregular", StringComparison.OrdinalIgnoreCase))
            {
                return "Irregular";
            }

            if (string.Equals(value, "Summer", StringComparison.OrdinalIgnoreCase))
            {
                return "Summer";
            }

            return "Regular";
        }

        private static bool IsValidStudentType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return string.Equals(value, "Regular", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "Irregular", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "Summer", StringComparison.OrdinalIgnoreCase);
        }

        public int Create(Student student)
        {
            return _studentData.Insert(student);
        }

        public void Update(Student student)
        {
            _studentData.Update(student);
        }

        public void UpdateLegacyProfile(Student student)
        {
            _studentData.UpdateLegacyProfile(student);
        }

        public void Delete(int studentId)
        {
            _studentData.Delete(studentId);
        }

        public byte[] GetPhotoData(int studentId)
        {
            return _studentData.GetPhotoData(studentId);
        }
    }
}

