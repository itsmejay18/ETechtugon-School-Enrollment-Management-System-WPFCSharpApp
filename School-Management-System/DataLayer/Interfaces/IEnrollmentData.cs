using System.Collections.Generic;
using System.Data;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IEnrollmentData
    {
        string GetNextEnrollmentNumber();
        int Insert(Enrollment enrollment, IList<EnrollmentDetail> details);
        DataTable GetPrerequisiteFailures(int studentId, IEnumerable<int> subjectIds);
        DataTable GetCapacityFailures(IEnumerable<int> subjectIds);
        DataTable GetScheduleConflicts(int studentId, IEnumerable<int> classScheduleIds, int academicYearId, int semesterId);
        bool IsStudentAlreadyEnrolledInSubject(int studentId, int subjectId, int academicYearId, int semesterId);
    }
}

