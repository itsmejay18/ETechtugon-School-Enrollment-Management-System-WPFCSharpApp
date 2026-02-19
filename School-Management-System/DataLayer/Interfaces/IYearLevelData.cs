using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IYearLevelData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        int Insert(YearLevel yearLevel);
        void Update(YearLevel yearLevel);
        void Delete(int yearLevelId);
    }
}
