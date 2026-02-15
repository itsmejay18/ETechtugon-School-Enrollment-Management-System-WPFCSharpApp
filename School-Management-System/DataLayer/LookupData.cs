using System;
using System.Data;
using School_Management_System.DataLayer.Interfaces;

namespace School_Management_System.DataLayer
{
    public sealed class LookupData : ILookupData
    {
        private readonly DatabaseHelper _db;

        public LookupData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetAcademicYears()
        {
            const string sql = @"
SELECT AcademicYearId, Name, IsCurrent
FROM AcademicYear
WHERE IsActive = 1
ORDER BY IsCurrent DESC, Name DESC;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetYearLevels()
        {
            const string sql = @"
SELECT YearLevelId, Name, SortOrder
FROM YearLevel
WHERE IsActive = 1
ORDER BY SortOrder, Name;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetSemesters()
        {
            const string sql = @"
SELECT SemesterId, Name, SortOrder
FROM Semester
WHERE IsActive = 1
ORDER BY SortOrder, Name;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }
    }
}
