using System.Data;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ILookupData
    {
        DataTable GetAcademicYears();
        DataTable GetYearLevels();
        DataTable GetSemesters();
    }
}

