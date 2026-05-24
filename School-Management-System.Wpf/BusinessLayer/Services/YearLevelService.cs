using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class YearLevelService
    {
        private readonly IYearLevelData _yearLevelData;

        public YearLevelService(IYearLevelData yearLevelData)
        {
            _yearLevelData = yearLevelData ?? throw new ArgumentNullException(nameof(yearLevelData));
        }

        public DataTable GetYearLevels(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return _yearLevelData.GetAllActive();
            }

            return _yearLevelData.Search(search);
        }

        public ValidationResult Validate(YearLevel yearLevel)
        {
            var vr = new ValidationResult();
            if (yearLevel == null)
            {
                vr.Add("Year Level is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(yearLevel.Name))
            {
                vr.Add("Name is required.");
            }

            if (yearLevel.SortOrder < 0)
            {
                vr.Add("Sort Order must be zero or positive.");
            }

            return vr;
        }

        public int Create(YearLevel yearLevel)
        {
            return _yearLevelData.Insert(yearLevel);
        }

        public void Update(YearLevel yearLevel)
        {
            _yearLevelData.Update(yearLevel);
        }

        public void Delete(int yearLevelId)
        {
            _yearLevelData.Delete(yearLevelId);
        }
    }
}
