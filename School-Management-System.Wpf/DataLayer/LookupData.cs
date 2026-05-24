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
FROM academicyear
WHERE IsActive = 1
ORDER BY IsCurrent DESC, Name DESC;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetYearLevels()
        {
            const string sql = @"
SELECT YearLevelId, Name, SortOrder
FROM yearlevel
WHERE IsActive = 1
ORDER BY SortOrder, Name;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }

        public DataTable GetSemesters()
        {
            const string sql = @"
SELECT SemesterId, Name, SortOrder
FROM semester
WHERE IsActive = 1
ORDER BY
    CASE
        WHEN Name = '1st Semester' THEN 1
        WHEN Name = '2nd Semester' THEN 2
        ELSE 100 + SortOrder
    END,
    SortOrder,
    SemesterId
LIMIT 2;";

            return _db.ExecuteDataTable(sql, CommandType.Text, null);
        }
    }
}
