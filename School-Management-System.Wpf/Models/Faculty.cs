using System;

namespace School_Management_System.Models
{
    public sealed class Faculty
    {
        private int _facultyId;
        private string _facultyCode;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _email;
        private string _phone;
        private string _address;
        private string _photoPath;
        private byte[] _photoData;
        private DateTime? _hireDate;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int FacultyId { get { return _facultyId; } set { _facultyId = value; } }
        public string FacultyCode { get { return _facultyCode; } set { _facultyCode = value; } }
        public string FirstName { get { return _firstName; } set { _firstName = value; } }
        public string LastName { get { return _lastName; } set { _lastName = value; } }
        public string MiddleName { get { return _middleName; } set { _middleName = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }
        public string Address { get { return _address; } set { _address = value; } }
        public string PhotoPath { get { return _photoPath; } set { _photoPath = value; } }
        public byte[] PhotoData { get { return _photoData; } set { _photoData = value; } }
        public DateTime? HireDate { get { return _hireDate; } set { _hireDate = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}

