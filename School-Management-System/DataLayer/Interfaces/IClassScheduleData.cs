using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IClassScheduleData
    {
        DataTable GetBySection(int sectionId);
        int Insert(ClassSchedule schedule);
        void Update(ClassSchedule schedule);
        void Delete(int classScheduleId);
    }
}
