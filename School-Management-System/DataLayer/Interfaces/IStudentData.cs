using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IStudentData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        string GetNextStudentNumber();
        int Insert(Student student);
        void Update(Student student);
        void Delete(int studentId);
        int GetActiveCount();
    }
}

