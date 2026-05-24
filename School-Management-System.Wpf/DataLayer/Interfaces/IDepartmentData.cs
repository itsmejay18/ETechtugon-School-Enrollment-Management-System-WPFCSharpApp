using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IDepartmentData
    {
        DataTable GetAllActive();
        DataTable GetLookupActive();
        DataTable Search(string query);
        int Insert(Department department);
        void Update(Department department);
        void Delete(int departmentId);
        int GetActiveCount();
    }
}
