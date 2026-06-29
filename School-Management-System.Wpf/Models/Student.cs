using System;

namespace School_Management_System.Models
{
    public sealed class Student
    {
        private int _studentId;
        private string _studentNumber;
        private string _studentType;
        private int? _curriculumId;
        private string _academicStatus;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _suffix;
        private string _gender;
        private DateTime? _birthDate;
        private string _birthPlace;
        private string _civilStatus;
        private string _citizenship;
        private string _religion;
        private string _email;
        private string _phone;
        private string _address;
        private string _zipCode;
        private string _fatherName;
        private string _motherName;
        private string _parentsAddress;
        private string _spouseName;
        private string _spouseAddress;
        private string _guardianName;
        private string _guardianContactNo;
        private string _nstpSerialNo;
        private string _paymentScheme;
        private string _currentSchoolYear;
        private string _currentSemester;
        private string _photoPath;
        private byte[] _photoData;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int StudentId { get { return _studentId; } set { _studentId = value; } }
        public string StudentNumber { get { return _studentNumber; } set { _studentNumber = value; } }
        public string StudentType { get { return _studentType; } set { _studentType = value; } }
        public int? CurriculumId { get { return _curriculumId; } set { _curriculumId = value; } }
        public string AcademicStatus { get { return _academicStatus; } set { _academicStatus = value; } }
        public string FirstName { get { return _firstName; } set { _firstName = value; } }
        public string LastName { get { return _lastName; } set { _lastName = value; } }
        public string MiddleName { get { return _middleName; } set { _middleName = value; } }
        public string Suffix { get { return _suffix; } set { _suffix = value; } }
        public string Gender { get { return _gender; } set { _gender = value; } }
        public DateTime? BirthDate { get { return _birthDate; } set { _birthDate = value; } }
        public string BirthPlace { get { return _birthPlace; } set { _birthPlace = value; } }
        public string CivilStatus { get { return _civilStatus; } set { _civilStatus = value; } }
        public string Citizenship { get { return _citizenship; } set { _citizenship = value; } }
        public string Religion { get { return _religion; } set { _religion = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }
        public string Address { get { return _address; } set { _address = value; } }
        public string ZipCode { get { return _zipCode; } set { _zipCode = value; } }
        public string FatherName { get { return _fatherName; } set { _fatherName = value; } }
        public string MotherName { get { return _motherName; } set { _motherName = value; } }
        public string ParentsAddress { get { return _parentsAddress; } set { _parentsAddress = value; } }
        public string SpouseName { get { return _spouseName; } set { _spouseName = value; } }
        public string SpouseAddress { get { return _spouseAddress; } set { _spouseAddress = value; } }
        public string GuardianName { get { return _guardianName; } set { _guardianName = value; } }
        public string GuardianContactNo { get { return _guardianContactNo; } set { _guardianContactNo = value; } }
        public string NstpSerialNo { get { return _nstpSerialNo; } set { _nstpSerialNo = value; } }
        public string PaymentScheme { get { return _paymentScheme; } set { _paymentScheme = value; } }
        public string CurrentSchoolYear { get { return _currentSchoolYear; } set { _currentSchoolYear = value; } }
        public string CurrentSemester { get { return _currentSemester; } set { _currentSemester = value; } }
        public string PhotoPath { get { return _photoPath; } set { _photoPath = value; } }
        public byte[] PhotoData { get { return _photoData; } set { _photoData = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}

