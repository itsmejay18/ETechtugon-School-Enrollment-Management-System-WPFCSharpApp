using System;
using System.Data;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class CurriculumService
    {
        private readonly ICurriculumData _curriculumData;

        public CurriculumService(ICurriculumData curriculumData)
        {
            _curriculumData = curriculumData ?? throw new ArgumentNullException(nameof(curriculumData));
        }

        public int EnsureCurriculum(string name, int courseId, int yearLevelId, int semesterId, int academicYearId)
        {
            var existingId = _curriculumData.GetCurriculumId(courseId, yearLevelId, semesterId, academicYearId);
            if (existingId.HasValue)
            {
                return existingId.Value;
            }

            return _curriculumData.InsertCurriculum(name, courseId, yearLevelId, semesterId, academicYearId);
        }

        public int? TryGetCurriculumId(int courseId, int yearLevelId, int semesterId, int academicYearId)
        {
            return _curriculumData.GetCurriculumId(courseId, yearLevelId, semesterId, academicYearId);
        }

        public DataTable GetSubjectsForCourse(int courseId)
        {
            return _curriculumData.GetSubjectsForCourse(courseId);
        }

        public DataTable GetCurriculumSubjects(int curriculumId)
        {
            return _curriculumData.GetCurriculumSubjects(curriculumId);
        }

        public void SetSubjectIncluded(int curriculumId, int subjectId, bool included)
        {
            if (included)
            {
                _curriculumData.AddSubject(curriculumId, subjectId);
            }
            else
            {
                _curriculumData.RemoveSubject(curriculumId, subjectId);
            }
        }
    }
}
