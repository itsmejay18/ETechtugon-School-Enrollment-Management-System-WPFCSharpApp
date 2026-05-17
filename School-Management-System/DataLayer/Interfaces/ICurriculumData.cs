using System.Data;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface ICurriculumData
    {
        int? GetCurriculumId(int courseId, int yearLevelId, int semesterId, int academicYearId);
        int? GetCurriculumId(int courseId, int yearLevelId, int semesterId, int academicYearId, string curriculumType);
        int InsertCurriculum(string name, int courseId, int yearLevelId, int semesterId, int academicYearId);
        int InsertCurriculum(string name, int courseId, int yearLevelId, int semesterId, int academicYearId, string curriculumType);

        DataTable GetSubjectsForCourse(int courseId);
        DataTable GetCurriculums(string search);
        DataTable GetCurriculumSubjects(int curriculumId);

        void AddSubject(int curriculumId, int subjectId);
        void RemoveSubject(int curriculumId, int subjectId);
    }
}

