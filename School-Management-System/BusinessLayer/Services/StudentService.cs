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

        public int Create(Student student)
        {
            return _studentData.Insert(student);
        }

        public void Update(Student student)
        {
            _studentData.Update(student);
        }

        public void Delete(int studentId)
        {
            _studentData.Delete(studentId);
        }
    }
}

