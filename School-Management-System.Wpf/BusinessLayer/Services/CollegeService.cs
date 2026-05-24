using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class CollegeService
    {
        private readonly ICollegeData _collegeData;

        public CollegeService(ICollegeData collegeData)
        {
            _collegeData = collegeData ?? throw new ArgumentNullException(nameof(collegeData));
        }

        public DataTable GetColleges(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return _collegeData.GetAllActive();
            }

            return _collegeData.Search(search);
        }

        public DataTable GetLookupColleges()
        {
            return _collegeData.GetLookupActive();
        }

        public int GetActiveCount()
        {
            return _collegeData.GetActiveCount();
        }

        public ValidationResult Validate(College college)
        {
            var vr = new ValidationResult();
            if (college == null)
            {
                vr.Add("College is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(college.CollegeCode))
            {
                vr.Add("College Code is required.");
            }

            if (string.IsNullOrWhiteSpace(college.CollegeName))
            {
                vr.Add("College Name is required.");
            }

            return vr;
        }

        public int Create(College college)
        {
            return _collegeData.Insert(college);
        }

        public void Update(College college)
        {
            _collegeData.Update(college);
        }

        public void Delete(int collegeId)
        {
            _collegeData.Delete(collegeId);
        }
    }
}
