using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IStudentData
    {
        DataTable GetAllActive();
        DataTable Search(string query);
        DataTable Search(string query, string studentType);
        string GetNextStudentNumber();
        int Insert(Student student);
        void Update(Student student);
        void Delete(int studentId);
        DataTable GetEnrolledSubjects(int studentId, int? academicYearId, int? semesterId);
        DataTable GetEnrollmentHistory(int studentId);
        DataTable GetCompletedSubjects(int studentId);
        DataTable GetFailedSubjects(int studentId);
        DataTable GetRemainingSubjects(int studentId, int? curriculumId);
        DataTable GetAcademicProfile(int studentId);
        int GetActiveCount();
        byte[] GetPhotoData(int studentId);
    }
}

