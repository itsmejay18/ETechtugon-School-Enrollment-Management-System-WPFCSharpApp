using System;
using System.Collections.Generic;

namespace School_Management_System.Models
{
    public sealed class IntegratedStudentEnrollmentRecord
    {
        public IntegratedStudentEnrollmentRecord()
        {
            Subjects = new List<IntegratedEnrollmentSubject>();
        }

        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string EnrollmentNumber { get; set; }
        public string CourseCode { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string YearLevel { get; set; }
        public string Section { get; set; }
        public string Status { get; set; }
        public int TotalUnits { get; set; }
        public IList<IntegratedEnrollmentSubject> Subjects { get; set; }
    }

    public sealed class IntegratedEnrollmentSubject
    {
        public string SubjectCode { get; set; }
        public string SubjectTitle { get; set; }
        public int Units { get; set; }
        public string Schedule { get; set; }
        public string Room { get; set; }
    }
}
