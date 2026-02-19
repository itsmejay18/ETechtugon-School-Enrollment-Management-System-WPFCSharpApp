using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class SectionService
    {
        private readonly ISectionData _sectionData;

        public SectionService(ISectionData sectionData)
        {
            _sectionData = sectionData ?? throw new ArgumentNullException(nameof(sectionData));
        }

        public DataTable GetSections(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return _sectionData.GetAllActive();
            }

            return _sectionData.Search(search);
        }

        public DataTable GetSectionsForTerm(int courseId, int academicYearId, int semesterId, int? yearLevelId)
        {
            return _sectionData.GetByCourseAndTerm(courseId, academicYearId, semesterId, yearLevelId);
        }

        public ValidationResult Validate(Section section)
        {
            var vr = new ValidationResult();
            if (section == null)
            {
                vr.Add("Section is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(section.SectionName))
            {
                vr.Add("Section Name is required.");
            }

            if (section.CourseId <= 0) vr.Add("Course is required.");
            if (section.YearLevelId <= 0) vr.Add("Year Level is required.");
            if (section.AcademicYearId <= 0) vr.Add("Academic Year is required.");
            if (section.SemesterId <= 0) vr.Add("Semester is required.");

            return vr;
        }

        public int Create(Section section)
        {
            return _sectionData.Insert(section);
        }

        public void Update(Section section)
        {
            _sectionData.Update(section);
        }

        public void Delete(int sectionId)
        {
            _sectionData.Delete(sectionId);
        }
    }
}
