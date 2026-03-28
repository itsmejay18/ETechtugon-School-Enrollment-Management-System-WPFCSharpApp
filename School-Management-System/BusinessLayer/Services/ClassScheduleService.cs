using System;
using System.Data;
using School_Management_System.BusinessLayer.Validation;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.BusinessLayer.Services
{
    public sealed class ClassScheduleService
    {
        private readonly IClassScheduleData _classScheduleData;

        public ClassScheduleService(IClassScheduleData classScheduleData)
        {
            _classScheduleData = classScheduleData ?? throw new ArgumentNullException(nameof(classScheduleData));
        }

        public DataTable GetBySection(int sectionId)
        {
            return _classScheduleData.GetBySection(sectionId);
        }

        public DataTable GetByFaculty(int facultyId)
        {
            return _classScheduleData.GetByFaculty(facultyId);
        }

        public ValidationResult Validate(ClassSchedule schedule)
        {
            var vr = new ValidationResult();
            if (schedule == null)
            {
                vr.Add("Schedule is required.");
                return vr;
            }

            if (schedule.SectionId <= 0) vr.Add("Section is required.");
            if (schedule.SubjectId <= 0) vr.Add("Subject is required.");
            if (schedule.AcademicYearId <= 0) vr.Add("Academic Year is required.");
            if (schedule.SemesterId <= 0) vr.Add("Semester is required.");

            if (schedule.StartTime.HasValue && schedule.EndTime.HasValue && schedule.StartTime >= schedule.EndTime)
            {
                vr.Add("Start Time must be before End Time.");
            }

            return vr;
        }

        public int Create(ClassSchedule schedule)
        {
            return _classScheduleData.Insert(schedule);
        }

        public void Update(ClassSchedule schedule)
        {
            _classScheduleData.Update(schedule);
        }

        public void Delete(int classScheduleId)
        {
            _classScheduleData.Delete(classScheduleId);
        }
    }
}
