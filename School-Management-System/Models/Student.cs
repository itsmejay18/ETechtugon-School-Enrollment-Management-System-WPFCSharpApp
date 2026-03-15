using System;

namespace School_Management_System.Models
{
    public sealed class Student
    {
        private int _studentId;
        private string _studentNumber;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private string _gender;
        private DateTime? _birthDate;
        private string _email;
        private string _phone;
        private string _address;
        private string _photoPath;
        private byte[] _photoData;
        private bool _isActive;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int StudentId { get { return _studentId; } set { _studentId = value; } }
        public string StudentNumber { get { return _studentNumber; } set { _studentNumber = value; } }
        public string FirstName { get { return _firstName; } set { _firstName = value; } }
        public string LastName { get { return _lastName; } set { _lastName = value; } }
        public string MiddleName { get { return _middleName; } set { _middleName = value; } }
        public string Gender { get { return _gender; } set { _gender = value; } }
        public DateTime? BirthDate { get { return _birthDate; } set { _birthDate = value; } }
        public string Email { get { return _email; } set { _email = value; } }
        public string Phone { get { return _phone; } set { _phone = value; } }
        public string Address { get { return _address; } set { _address = value; } }
        public string PhotoPath { get { return _photoPath; } set { _photoPath = value; } }
        public byte[] PhotoData { get { return _photoData; } set { _photoData = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public DateTime CreatedAt { get { return _createdAt; } set { _createdAt = value; } }
        public DateTime? UpdatedAt { get { return _updatedAt; } set { _updatedAt = value; } }
    }
}

