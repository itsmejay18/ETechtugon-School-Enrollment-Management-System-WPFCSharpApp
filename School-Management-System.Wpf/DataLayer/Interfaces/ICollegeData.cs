using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ICollegeData
    {
        DataTable GetAllActive();
        DataTable GetLookupActive();
        DataTable Search(string query);
        int Insert(College college);
        void Update(College college);
        void Delete(int collegeId);
        int GetActiveCount();
    }
}
