using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class SubjectService
    {
        private readonly ISubjectData _subjectData;

        public SubjectService(ISubjectData subjectData)
        {
            _subjectData = subjectData ?? throw new ArgumentNullException(nameof(subjectData));
        }

        public DataTable GetSubjects(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _subjectData.GetAllActive();
            }

            return _subjectData.Search(searchText);
        }

        public DataTable GetSubjectsForCourse(int courseId)
        {
            return _subjectData.GetByCourse(courseId);
        }

        public int GetActiveCount()
        {
            return _subjectData.GetActiveCount();
        }

        public ValidationResult Validate(Subject subject)
        {
            var vr = new ValidationResult();
            if (subject == null)
            {
                vr.Add("Subject is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(subject.SubjectCode))
            {
                vr.Add("Subject Code is required.");
            }

            if (string.IsNullOrWhiteSpace(subject.SubjectName))
            {
                vr.Add("Subject Name is required.");
            }

            if (subject.Units <= 0)
            {
                vr.Add("Units must be greater than 0.");
            }

            return vr;
        }

        public int Create(Subject subject)
        {
            return _subjectData.Insert(subject);
        }

        public void Update(Subject subject)
        {
            _subjectData.Update(subject);
        }

        public void Delete(int subjectId)
        {
            _subjectData.Delete(subjectId);
        }
    }
}

