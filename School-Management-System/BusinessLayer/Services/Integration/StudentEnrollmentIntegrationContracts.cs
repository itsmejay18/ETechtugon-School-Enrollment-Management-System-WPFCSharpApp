using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace School_Management_System.BusinessLayer.Services.Integration
{
    [XmlRoot("StudentProfile")]
    public sealed class StudentProfileXmlData
    {
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    [DataContract]
    public sealed class EnrollmentRecordJsonData
    {
        public EnrollmentRecordJsonData()
        {
            Subjects = new List<EnrollmentSubjectJsonData>();
        }

        [DataMember(Name = "studentNumber")]
        public string StudentNumber { get; set; }

        [DataMember(Name = "enrollmentNumber")]
        public string EnrollmentNumber { get; set; }

        [DataMember(Name = "courseCode")]
        public string CourseCode { get; set; }

        [DataMember(Name = "academicYear")]
        public string AcademicYear { get; set; }

        [DataMember(Name = "semester")]
        public string Semester { get; set; }

        [DataMember(Name = "yearLevel")]
        public string YearLevel { get; set; }

        [DataMember(Name = "section")]
        public string Section { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "subjects")]
        public List<EnrollmentSubjectJsonData> Subjects { get; set; }
    }

    [DataContract]
    public sealed class EnrollmentSubjectJsonData
    {
        [DataMember(Name = "subjectCode")]
        public string SubjectCode { get; set; }

        [DataMember(Name = "subjectTitle")]
        public string SubjectTitle { get; set; }

        [DataMember(Name = "units")]
        public int Units { get; set; }

        [DataMember(Name = "schedule")]
        public string Schedule { get; set; }

        [DataMember(Name = "room")]
        public string Room { get; set; }
    }
}
