using System;
using System.Collections.Generic;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class EnrollmentService
    {
        private readonly IEnrollmentData _enrollmentData;

        public EnrollmentService(IEnrollmentData enrollmentData)
        {
            _enrollmentData = enrollmentData ?? throw new ArgumentNullException(nameof(enrollmentData));
        }

        public string GetNextEnrollmentNumber()
        {
            return _enrollmentData.GetNextEnrollmentNumber();
        }

        public ValidationResult Validate(Enrollment enrollment, IList<EnrollmentDetail> details)
        {
            var vr = new ValidationResult();

            if (enrollment == null)
            {
                vr.Add("Enrollment is required.");
                return vr;
            }

            if (enrollment.StudentId <= 0) vr.Add("Student is required.");
            if (enrollment.CourseId <= 0) vr.Add("Course is required.");
            if (enrollment.AcademicYearId <= 0) vr.Add("Academic Year is required.");
            if (enrollment.YearLevelId <= 0) vr.Add("Year Level is required.");
            if (enrollment.SemesterId <= 0) vr.Add("Semester is required.");
            if (enrollment.SectionId <= 0) vr.Add("Section is required.");

            if (details == null || details.Count == 0)
            {
                vr.Add("Please select at least one subject.");
            }

            return vr;
        }

        public int Save(Enrollment enrollment, IList<EnrollmentDetail> details)
        {
            if (enrollment == null) throw new ArgumentNullException(nameof(enrollment));
            if (details == null) throw new ArgumentNullException(nameof(details));

            var totalUnits = 0;
            foreach (var d in details)
            {
                totalUnits += d.Units;
            }

            enrollment.TotalUnits = totalUnits;
            if (string.IsNullOrWhiteSpace(enrollment.Status))
            {
                enrollment.Status = "Posted";
            }

            return _enrollmentData.Insert(enrollment, details);
        }
    }
}

