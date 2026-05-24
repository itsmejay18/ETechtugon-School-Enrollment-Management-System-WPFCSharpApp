using System;
using System.Collections.Generic;
using School_Management_System.Models;

namespace School_Management_System.DataLayer.Interfaces
{
    public interface IActivityLogData
    {
        void Add(ActivityLog entry);
        IList<ActivityLog> Search(DateTime? fromUtcInclusive, DateTime? toUtcExclusive, string usernameLike, string action, int maxRows);
    }
}
