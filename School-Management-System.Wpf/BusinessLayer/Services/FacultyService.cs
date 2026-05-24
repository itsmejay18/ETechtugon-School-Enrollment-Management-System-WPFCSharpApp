using System;
using System.Data;
using System.Text.RegularExpressions;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class FacultyService
    {
        private readonly IFacultyData _facultyData;

        public FacultyService(IFacultyData facultyData)
        {
            _facultyData = facultyData ?? throw new ArgumentNullException(nameof(facultyData));
        }

        public DataTable GetFaculty(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _facultyData.GetAllActive();
            }

            return _facultyData.Search(searchText);
        }

        public string GetNextFacultyCode()
        {
            return _facultyData.GetNextFacultyCode();
        }

        public int GetActiveCount()
        {
            return _facultyData.GetActiveCount();
        }

        public ValidationResult Validate(Faculty faculty)
        {
            var vr = new ValidationResult();
            if (faculty == null)
            {
                vr.Add("Faculty is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(faculty.FacultyCode))
            {
                vr.Add("Faculty Code is required.");
            }

            if (string.IsNullOrWhiteSpace(faculty.FirstName))
            {
                vr.Add("First Name is required.");
            }

            if (string.IsNullOrWhiteSpace(faculty.LastName))
            {
                vr.Add("Last Name is required.");
            }

            if (!string.IsNullOrWhiteSpace(faculty.Email))
            {
                if (!Regex.IsMatch(faculty.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    vr.Add("Email is invalid.");
                }
            }

            return vr;
        }

        public int Create(Faculty faculty)
        {
            return _facultyData.Insert(faculty);
        }

        public void Update(Faculty faculty)
        {
            _facultyData.Update(faculty);
        }

        public void Delete(int facultyId)
        {
            _facultyData.Delete(facultyId);
        }

        public byte[] GetPhotoData(int facultyId)
        {
            return _facultyData.GetPhotoData(facultyId);
        }
    }
}

