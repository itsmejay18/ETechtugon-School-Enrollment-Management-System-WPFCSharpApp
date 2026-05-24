using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ICourseData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        DataTable GetLookupActive();
        int Insert(Course course);
        void Update(Course course);
        void Delete(int courseId);
        int GetActiveCount();
    }
}

