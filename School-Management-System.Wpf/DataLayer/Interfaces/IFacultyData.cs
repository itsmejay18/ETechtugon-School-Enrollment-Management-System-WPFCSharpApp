using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IFacultyData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        string GetNextFacultyCode();
        int Insert(Faculty faculty);
        void Update(Faculty faculty);
        void Delete(int facultyId);
        int GetActiveCount();
        byte[] GetPhotoData(int facultyId);
    }
}

