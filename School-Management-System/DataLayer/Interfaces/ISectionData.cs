using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ISectionData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        DataTable Search(string query, int? courseId);
        DataTable GetByCourseAndTerm(int courseId, int academicYearId, int semesterId, int? yearLevelId);
        int Insert(Section section);
        void Update(Section section);
        void Delete(int sectionId);
    }
}
