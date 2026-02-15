using System.Collections.Generic;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IEnrollmentData
    {
        string GetNextEnrollmentNumber();
        int Insert(Enrollment enrollment, IList<EnrollmentDetail> details);
    }
}

