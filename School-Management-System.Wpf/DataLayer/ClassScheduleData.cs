using System;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class ClassScheduleData : IClassScheduleData
    {
        private readonly DatabaseHelper _db;

        public ClassScheduleData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public DataTable GetBySection(int sectionId)
        {
            return GetBySection(sectionId, null, null);
        }

        public DataTable GetBySection(int sectionId, int? academicYearId, int? semesterId)
        {
            const string sql = @"
SELECT
    cs.ClassScheduleId,
    cs.SectionId,
    sec.SectionCode,
    sec.SectionName,
    cs.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
    s.MaxStudents,
    s.CurrentEnrolledCount,
    cs.FacultyId,
    CONCAT(f.LastName, ', ', f.FirstName) AS FacultyName,
    cs.DayOfWeek,
    cs.StartTime,
    cs.EndTime,
    cs.Room,
    cs.Remarks,
    cs.AcademicYearId,
    ay.Name AS AcademicYear,
    cs.SemesterId,
    sem.Name AS Semester
FROM classschedule cs
INNER JOIN section sec ON sec.SectionId = cs.SectionId
INNER JOIN subject s ON s.SubjectId = cs.SubjectId
LEFT JOIN faculty f ON f.FacultyId = cs.FacultyId
INNER JOIN academicyear ay ON ay.AcademicYearId = cs.AcademicYearId
INNER JOIN semester sem ON sem.SemesterId = cs.SemesterId
WHERE cs.IsActive = 1
  AND cs.SectionId = @SectionId
  AND (@AcademicYearId IS NULL OR cs.AcademicYearId = @AcademicYearId)
  AND (@SemesterId IS NULL OR cs.SemesterId = @SemesterId)
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SectionId", sectionId),
                    new MySqlParameter("@AcademicYearId", (object)academicYearId ?? DBNull.Value),
                    new MySqlParameter("@SemesterId", (object)semesterId ?? DBNull.Value)
                });
        }

        public DataTable GetByFaculty(int facultyId)
        {
            return GetByFaculty(facultyId, null, null);
        }

        public DataTable GetByFaculty(int facultyId, int? academicYearId, int? semesterId)
        {
            const string sql = @"
SELECT
    cs.ClassScheduleId,
    s.SubjectCode,
    s.SubjectName,
    sec.SectionCode,
    sec.SectionName,
    cs.DayOfWeek,
    cs.StartTime,
    cs.EndTime,
    cs.Room,
    ay.Name AS AcademicYear,
    sem.Name AS Semester
FROM classschedule cs
INNER JOIN subject s ON s.SubjectId = cs.SubjectId
LEFT JOIN section sec ON sec.SectionId = cs.SectionId
LEFT JOIN academicyear ay ON ay.AcademicYearId = cs.AcademicYearId
LEFT JOIN semester sem ON sem.SemesterId = cs.SemesterId
WHERE cs.IsActive = 1
  AND cs.FacultyId = @FacultyId
  AND (@AcademicYearId IS NULL OR cs.AcademicYearId = @AcademicYearId)
  AND (@SemesterId IS NULL OR cs.SemesterId = @SemesterId)
ORDER BY ay.Name DESC, sem.Name DESC, s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@FacultyId", facultyId),
                    new MySqlParameter("@AcademicYearId", (object)academicYearId ?? DBNull.Value),
                    new MySqlParameter("@SemesterId", (object)semesterId ?? DBNull.Value)
                });
        }

        public int Insert(ClassSchedule schedule)
        {
            Guard.NotNull(schedule, nameof(schedule));

            const string sql = @"
INSERT INTO classschedule
(
    SectionId,
    SubjectId,
    FacultyId,
    DayOfWeek,
    StartTime,
    EndTime,
    Room,
    Remarks,
    AcademicYearId,
    SemesterId,
    IsActive,
    CreatedAt
)
VALUES
(
    @SectionId,
    @SubjectId,
    @FacultyId,
    @DayOfWeek,
    @StartTime,
    @EndTime,
    @Room,
    @Remarks,
    @AcademicYearId,
    @SemesterId,
    1,
    UTC_TIMESTAMP()
);";

            var id = _db.ExecuteInsert(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@SectionId", schedule.SectionId),
                    new MySqlParameter("@SubjectId", schedule.SubjectId),
                    new MySqlParameter("@FacultyId", (object)schedule.FacultyId ?? DBNull.Value),
                    new MySqlParameter("@DayOfWeek", (object)schedule.DayOfWeek ?? DBNull.Value),
                    new MySqlParameter("@StartTime", (object)schedule.StartTime ?? DBNull.Value),
                    new MySqlParameter("@EndTime", (object)schedule.EndTime ?? DBNull.Value),
                    new MySqlParameter("@Room", (object)schedule.Room ?? DBNull.Value),
                    new MySqlParameter("@Remarks", (object)schedule.Remarks ?? DBNull.Value),
                    new MySqlParameter("@AcademicYearId", schedule.AcademicYearId),
                    new MySqlParameter("@SemesterId", schedule.SemesterId)
                });

            return Convert.ToInt32(id);
        }

        public void Update(ClassSchedule schedule)
        {
            Guard.NotNull(schedule, nameof(schedule));

            const string sql = @"
UPDATE classschedule
SET
    SectionId = @SectionId,
    SubjectId = @SubjectId,
    FacultyId = @FacultyId,
    DayOfWeek = @DayOfWeek,
    StartTime = @StartTime,
    EndTime = @EndTime,
    Room = @Room,
    Remarks = @Remarks,
    AcademicYearId = @AcademicYearId,
    SemesterId = @SemesterId,
    UpdatedAt = UTC_TIMESTAMP()
WHERE ClassScheduleId = @ClassScheduleId;";

            _db.ExecuteNonQuery(
                sql,
                CommandType.Text,
                new[]
                {
                    new MySqlParameter("@ClassScheduleId", schedule.ClassScheduleId),
                    new MySqlParameter("@SectionId", schedule.SectionId),
                    new MySqlParameter("@SubjectId", schedule.SubjectId),
                    new MySqlParameter("@FacultyId", (object)schedule.FacultyId ?? DBNull.Value),
                    new MySqlParameter("@DayOfWeek", (object)schedule.DayOfWeek ?? DBNull.Value),
                    new MySqlParameter("@StartTime", (object)schedule.StartTime ?? DBNull.Value),
                    new MySqlParameter("@EndTime", (object)schedule.EndTime ?? DBNull.Value),
                    new MySqlParameter("@Room", (object)schedule.Room ?? DBNull.Value),
                    new MySqlParameter("@Remarks", (object)schedule.Remarks ?? DBNull.Value),
                    new MySqlParameter("@AcademicYearId", schedule.AcademicYearId),
                    new MySqlParameter("@SemesterId", schedule.SemesterId)
                });
        }

        public void Delete(int classScheduleId)
        {
            const string sql = @"UPDATE classschedule SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE ClassScheduleId = @ClassScheduleId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@ClassScheduleId", classScheduleId) });
        }
    }
}

