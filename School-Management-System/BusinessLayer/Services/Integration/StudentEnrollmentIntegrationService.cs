using System;
using System.IO;
using System.Linq;
using School_Management_System.Common;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services.Integration
{
    public sealed class StudentEnrollmentIntegrationService
    {
        private readonly IStudentProfileXmlReader _studentProfileXmlReader;
        private readonly IEnrollmentRecordJsonReader _enrollmentRecordJsonReader;

        public StudentEnrollmentIntegrationService()
            : this(new StudentProfileXmlReader(), new EnrollmentRecordJsonReader())
        {
        }

        internal StudentEnrollmentIntegrationService(
            IStudentProfileXmlReader studentProfileXmlReader,
            IEnrollmentRecordJsonReader enrollmentRecordJsonReader)
        {
            _studentProfileXmlReader = studentProfileXmlReader ?? throw new ArgumentNullException(nameof(studentProfileXmlReader));
            _enrollmentRecordJsonReader = enrollmentRecordJsonReader ?? throw new ArgumentNullException(nameof(enrollmentRecordJsonReader));
        }

        public IntegratedStudentEnrollmentRecord MergeFromFiles(string xmlFilePath, string jsonFilePath)
        {
            var studentProfile = _studentProfileXmlReader.ReadFromFile(xmlFilePath);
            var enrollmentRecord = _enrollmentRecordJsonReader.ReadFromFile(jsonFilePath);
            return Merge(studentProfile, enrollmentRecord);
        }

        public IntegratedStudentEnrollmentRecord MergeFromContent(string xmlContent, string jsonContent)
        {
            var studentProfile = _studentProfileXmlReader.ReadFromContent(xmlContent);
            var enrollmentRecord = _enrollmentRecordJsonReader.ReadFromContent(jsonContent);
            return Merge(studentProfile, enrollmentRecord);
        }

        private static IntegratedStudentEnrollmentRecord Merge(
            StudentProfileXmlData studentProfile,
            EnrollmentRecordJsonData enrollmentRecord)
        {
            Guard.NotNull(studentProfile, nameof(studentProfile));
            Guard.NotNull(enrollmentRecord, nameof(enrollmentRecord));

            var xmlStudentNumber = NormalizeKey(studentProfile.StudentNumber);
            var jsonStudentNumber = NormalizeKey(enrollmentRecord.StudentNumber);

            if (string.IsNullOrWhiteSpace(xmlStudentNumber))
            {
                throw new InvalidDataException("Student XML is missing StudentNumber.");
            }

            if (string.IsNullOrWhiteSpace(jsonStudentNumber))
            {
                throw new InvalidDataException("Enrollment JSON is missing studentNumber.");
            }

            if (!string.Equals(xmlStudentNumber, jsonStudentNumber, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Student XML and enrollment JSON refer to different students.");
            }

            var subjects = (enrollmentRecord.Subjects ?? Enumerable.Empty<EnrollmentSubjectJsonData>())
                .Select(MapSubject)
                .ToList();

            return new IntegratedStudentEnrollmentRecord
            {
                StudentNumber = xmlStudentNumber,
                FullName = BuildFullName(studentProfile.FirstName, studentProfile.MiddleName, studentProfile.LastName),
                FirstName = NormalizeValue(studentProfile.FirstName),
                LastName = NormalizeValue(studentProfile.LastName),
                MiddleName = NormalizeValue(studentProfile.MiddleName),
                Gender = NormalizeValue(studentProfile.Gender),
                BirthDate = studentProfile.BirthDate,
                Email = NormalizeValue(studentProfile.Email),
                Phone = NormalizeValue(studentProfile.Phone),
                Address = NormalizeValue(studentProfile.Address),
                EnrollmentNumber = NormalizeValue(enrollmentRecord.EnrollmentNumber),
                CourseCode = NormalizeValue(enrollmentRecord.CourseCode),
                AcademicYear = NormalizeValue(enrollmentRecord.AcademicYear),
                Semester = NormalizeValue(enrollmentRecord.Semester),
                YearLevel = NormalizeValue(enrollmentRecord.YearLevel),
                Section = NormalizeValue(enrollmentRecord.Section),
                Status = NormalizeValue(enrollmentRecord.Status),
                TotalUnits = subjects.Sum(subject => subject.Units),
                Subjects = subjects
            };
        }

        private static IntegratedEnrollmentSubject MapSubject(EnrollmentSubjectJsonData subject)
        {
            if (subject == null)
            {
                return new IntegratedEnrollmentSubject();
            }

            return new IntegratedEnrollmentSubject
            {
                SubjectCode = NormalizeValue(subject.SubjectCode),
                SubjectTitle = NormalizeValue(subject.SubjectTitle),
                Units = subject.Units,
                Schedule = NormalizeValue(subject.Schedule),
                Room = NormalizeValue(subject.Room)
            };
        }

        private static string BuildFullName(string firstName, string middleName, string lastName)
        {
            return string.Join(
                " ",
                new[] { firstName, middleName, lastName }
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(part => part.Trim()));
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
