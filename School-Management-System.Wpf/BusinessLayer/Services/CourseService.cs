using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class CourseService
    {
        private readonly ICourseData _courseData;

        public CourseService(ICourseData courseData)
        {
            _courseData = courseData ?? throw new ArgumentNullException(nameof(courseData));
        }

        public DataTable GetCourses(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _courseData.GetAllActive();
            }

            return _courseData.Search(searchText);
        }

        public DataTable GetLookupCourses()
        {
            return _courseData.GetLookupActive();
        }

        public int GetActiveCount()
        {
            return _courseData.GetActiveCount();
        }

        public ValidationResult Validate(Course course)
        {
            var vr = new ValidationResult();
            if (course == null)
            {
                vr.Add("Course is required.");
                return vr;
            }

            if (string.IsNullOrWhiteSpace(course.CourseCode))
            {
                vr.Add("Course Code is required.");
            }

            if (string.IsNullOrWhiteSpace(course.CourseName))
            {
                vr.Add("Course Name is required.");
            }

            return vr;
        }

        public int Create(Course course)
        {
            return _courseData.Insert(course);
        }

        public void Update(Course course)
        {
            _courseData.Update(course);
        }

        public void Delete(int courseId)
        {
            _courseData.Delete(courseId);
        }
    }
}

