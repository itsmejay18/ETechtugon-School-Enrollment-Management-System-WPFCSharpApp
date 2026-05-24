using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            if (!IsValidStudentType(enrollment.StudentType))
            {
                vr.Add("Student Type must be Regular, Irregular, or Summer.");
            }

            if (details == null || details.Count == 0)
            {
                vr.Add("Please select at least one subject.");
            }

            return vr;
        }

        public ValidationResult ValidateEnrollmentRules(Enrollment enrollment, IList<EnrollmentDetail> details)
        {
            var vr = Validate(enrollment, details);
            if (!vr.IsValid || enrollment == null || details == null)
            {
                return vr;
            }

            var subjectIds = details.Select(d => d.SubjectId).Where(id => id > 0).Distinct().ToList();
            var classScheduleIds = details
                .Where(d => d.ClassScheduleId.HasValue && d.ClassScheduleId.Value > 0)
                .Select(d => d.ClassScheduleId.Value)
                .Distinct()
                .ToList();

            foreach (var subjectId in subjectIds)
            {
                if (_enrollmentData.IsStudentAlreadyEnrolledInSubject(
                    enrollment.StudentId,
                    subjectId,
                    enrollment.AcademicYearId,
                    enrollment.SemesterId))
                {
                    vr.Add("Student is already enrolled in one or more selected subjects for the selected term.");
                    break;
                }
            }

            AddPrerequisiteFailures(vr, _enrollmentData.GetPrerequisiteFailures(enrollment.StudentId, subjectIds));
            AddCapacityFailures(vr, _enrollmentData.GetCapacityFailures(subjectIds));
            AddScheduleConflicts(vr, _enrollmentData.GetScheduleConflicts(
                enrollment.StudentId,
                classScheduleIds,
                enrollment.AcademicYearId,
                enrollment.SemesterId));

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

            enrollment.StudentType = NormalizeStudentType(enrollment.StudentType);

            return _enrollmentData.Insert(enrollment, details);
        }

        private static void AddPrerequisiteFailures(ValidationResult vr, DataTable failures)
        {
            AddFailureRows(
                vr,
                failures,
                "Prerequisite missing: {0} requires {1}.",
                "SubjectCode",
                "PrerequisiteCode");
        }

        private static void AddCapacityFailures(ValidationResult vr, DataTable failures)
        {
            AddFailureRows(
                vr,
                failures,
                "Subject capacity reached: {0} is full.",
                "SubjectCode",
                null);
        }

        private static void AddScheduleConflicts(ValidationResult vr, DataTable failures)
        {
            AddFailureRows(
                vr,
                failures,
                "Schedule conflict: {0} overlaps with {1}.",
                "SubjectCode",
                "ExistingSubjectCode");
        }

        private static void AddFailureRows(ValidationResult vr, DataTable table, string format, string primaryColumn, string secondaryColumn)
        {
            if (vr == null || table == null)
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                var primary = table.Columns.Contains(primaryColumn) ? Convert.ToString(row[primaryColumn]) : string.Empty;
                var secondary = !string.IsNullOrWhiteSpace(secondaryColumn) && table.Columns.Contains(secondaryColumn)
                    ? Convert.ToString(row[secondaryColumn])
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(secondaryColumn))
                {
                    vr.Add(string.Format(format, primary));
                }
                else
                {
                    vr.Add(string.Format(format, primary, secondary));
                }
            }
        }

        private static string NormalizeStudentType(string value)
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
    }
}

