using System;
using System.IO;
using NUnit.Framework;
using School_Management_System.BusinessLayer.Services.Integration;

namespace School_Management_System.Tests.Services
{
    [TestFixture]
    public sealed class StudentEnrollmentIntegrationServiceTests
    {
        private const string ValidXml = @"<StudentProfile>
  <StudentNumber>2026-0001</StudentNumber>
  <FirstName>Jana</FirstName>
  <LastName>Santos</LastName>
  <MiddleName>Marie</MiddleName>
  <Gender>Female</Gender>
  <BirthDate>2004-08-15T00:00:00</BirthDate>
  <Email>jana.santos@example.edu</Email>
  <Phone>09171234567</Phone>
  <Address>San Fernando, Pampanga</Address>
</StudentProfile>";

        private const string ValidJson = @"{
  ""studentNumber"": ""2026-0001"",
  ""enrollmentNumber"": ""ENR-2026-0001"",
  ""courseCode"": ""BSIT"",
  ""academicYear"": ""2025-2026"",
  ""semester"": ""1st Semester"",
  ""yearLevel"": ""4th Year"",
  ""section"": ""IT-4A"",
  ""status"": ""Posted"",
  ""subjects"": [
    {
      ""subjectCode"": ""IT401"",
      ""subjectTitle"": ""Advanced System Integration"",
      ""units"": 3,
      ""schedule"": ""MWF 09:00-10:00"",
      ""room"": ""Lab 2""
    },
    {
      ""subjectCode"": ""IT402"",
      ""subjectTitle"": ""Capstone Project"",
      ""units"": 3,
      ""schedule"": ""TTh 13:00-14:30"",
      ""room"": ""Room 305""
    }
  ]
}";

        private const string MismatchedJson = @"{
  ""studentNumber"": ""2026-9999"",
  ""enrollmentNumber"": ""ENR-2026-9999"",
  ""courseCode"": ""BSIT"",
  ""subjects"": []
}";

        [Test]
        public void MergeFromContent_WithMatchingXmlAndJson_ReturnsUnifiedRecord()
        {
            var service = new StudentEnrollmentIntegrationService();

            var result = service.MergeFromContent(ValidXml, ValidJson);

            Assert.That(result.StudentNumber, Is.EqualTo("2026-0001"));
            Assert.That(result.FullName, Is.EqualTo("Jana Marie Santos"));
            Assert.That(result.CourseCode, Is.EqualTo("BSIT"));
            Assert.That(result.TotalUnits, Is.EqualTo(6));
            Assert.That(result.Subjects, Has.Count.EqualTo(2));
            Assert.That(result.Subjects[0].SubjectCode, Is.EqualTo("IT401"));
        }

        [Test]
        public void MergeFromFiles_WithMatchingFiles_ReturnsUnifiedRecord()
        {
            var service = new StudentEnrollmentIntegrationService();
            var tempXmlPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
            var tempJsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

            try
            {
                File.WriteAllText(tempXmlPath, ValidXml);
                File.WriteAllText(tempJsonPath, ValidJson);

                var result = service.MergeFromFiles(tempXmlPath, tempJsonPath);

                Assert.That(result.EnrollmentNumber, Is.EqualTo("ENR-2026-0001"));
                Assert.That(result.Subjects, Has.Count.EqualTo(2));
            }
            finally
            {
                if (File.Exists(tempXmlPath))
                {
                    File.Delete(tempXmlPath);
                }

                if (File.Exists(tempJsonPath))
                {
                    File.Delete(tempJsonPath);
                }
            }
        }

        [Test]
        public void MergeFromContent_WithMalformedXml_ThrowsInvalidDataException()
        {
            var service = new StudentEnrollmentIntegrationService();
            const string malformedXml = "<StudentProfile><StudentNumber>2026-0001</StudentNumber>";

            Assert.That(
                () => service.MergeFromContent(malformedXml, ValidJson),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("well-formed"));
        }

        [Test]
        public void MergeFromContent_WithInvalidJson_ThrowsInvalidDataException()
        {
            var service = new StudentEnrollmentIntegrationService();
            const string invalidJson = "{ bad json }";

            Assert.That(
                () => service.MergeFromContent(ValidXml, invalidJson),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("Enrollment JSON is invalid"));
        }

        [Test]
        public void MergeFromContent_WithMismatchedStudentNumbers_ThrowsInvalidDataException()
        {
            var service = new StudentEnrollmentIntegrationService();

            Assert.That(
                () => service.MergeFromContent(ValidXml, MismatchedJson),
                Throws.TypeOf<InvalidDataException>().With.Message.Contains("different students"));
        }
    }
}
