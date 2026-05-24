using System;
using System.Data;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class LookupService
    {
        private readonly ILookupData _lookupData;

        public LookupService(ILookupData lookupData)
        {
            _lookupData = lookupData ?? throw new ArgumentNullException(nameof(lookupData));
        }

        public DataTable GetAcademicYears()
        {
            return _lookupData.GetAcademicYears();
        }

        public DataTable GetYearLevels()
        {
            return _lookupData.GetYearLevels();
        }

        public DataTable GetSemesters()
        {
            return _lookupData.GetSemesters();
        }
    }
}

