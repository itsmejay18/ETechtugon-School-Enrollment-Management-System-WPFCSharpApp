using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ISubjectData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        DataTable GetByCourse(int courseId);
        int Insert(Subject subject);
        void Update(Subject subject);
        void Delete(int subjectId);
        int GetActiveCount();
    }
}

