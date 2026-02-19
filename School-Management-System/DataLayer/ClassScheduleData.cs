using System;
using System.Data;
using MySql.Data.MySqlClient;
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
            const string sql = @"
SELECT
    cs.ClassScheduleId,
    cs.SectionId,
    sec.SectionName,
    cs.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    s.Units,
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
FROM ClassSchedule cs
INNER JOIN Section sec ON sec.SectionId = cs.SectionId
INNER JOIN Subject s ON s.SubjectId = cs.SubjectId
LEFT JOIN Faculty f ON f.FacultyId = cs.FacultyId
INNER JOIN AcademicYear ay ON ay.AcademicYearId = cs.AcademicYearId
INNER JOIN Semester sem ON sem.SemesterId = cs.SemesterId
WHERE cs.IsActive = 1
  AND cs.SectionId = @SectionId
ORDER BY s.SubjectName;";

            return _db.ExecuteDataTable(
                sql,
                CommandType.Text,
                new[] { new MySqlParameter("@SectionId", sectionId) });
        }

        public int Insert(ClassSchedule schedule)
        {
            Guard.NotNull(schedule, nameof(schedule));

            const string sql = @"
INSERT INTO ClassSchedule
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
UPDATE ClassSchedule
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
            const string sql = @"UPDATE ClassSchedule SET IsActive = 0, UpdatedAt = UTC_TIMESTAMP() WHERE ClassScheduleId = @ClassScheduleId;";
            _db.ExecuteNonQuery(sql, CommandType.Text, new[] { new MySqlParameter("@ClassScheduleId", classScheduleId) });
        }
    }
}
