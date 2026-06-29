using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using School_Management_System.Common;
using School_Management_System.DataLayer.Interfaces;
using School_Management_System.Models;

namespace School_Management_System.DataLayer
{
    public sealed class EnrollmentData : IEnrollmentData
    {
        private readonly DatabaseHelper _db;

        public EnrollmentData(DatabaseHelper db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public string GetNextEnrollmentNumber()
        {
            const string sql = @"
SELECT CONCAT(
    'ENR-',
    YEAR(UTC_DATE()),
    '-',
    LPAD(IFNULL(MAX(CAST(RIGHT(EnrollmentNumber, 4) AS UNSIGNED)), 0) + 1, 4, '0')
)
FROM enrollment
WHERE EnrollmentNumber LIKE CONCAT('ENR-', YEAR(UTC_DATE()), '-', '____');";

            return Convert.ToString(_db.ExecuteScalar(sql, CommandType.Text, null));
        }

        public int Insert(Enrollment enrollment, IList<EnrollmentDetail> details)
        {
            Guard.NotNull(enrollment, nameof(enrollment));
            Guard.NotNull(details, nameof(details));

            var enrollmentId = 0;

            _db.ExecuteInTransaction((conn, tx) =>
            {
                const string sqlEnrollment = @"
INSERT INTO enrollment
(
    EnrollmentNumber,
    StudentId,
    CourseId,
    AcademicYearId,
    YearLevelId,
    SemesterId,
    SectionId,
    CurriculumId,
    StudentType,
    ScholarshipStatus,
    EnrollDate,
    TotalUnits,
    `Status`,
    CreatedAt
)
VALUES
(
    @EnrollmentNumber,
    @StudentId,
    @CourseId,
    @AcademicYearId,
    @YearLevelId,
    @SemesterId,
    @SectionId,
    @CurriculumId,
    @StudentType,
    @ScholarshipStatus,
    @EnrollDate,
    @TotalUnits,
    @Status,
    UTC_TIMESTAMP()
);";

                using (var cmd = new MySqlCommand(sqlEnrollment, conn, tx))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new MySqlParameter("@EnrollmentNumber", (object)enrollment.EnrollmentNumber ?? DBNull.Value));
                    cmd.Parameters.Add(new MySqlParameter("@StudentId", enrollment.StudentId));
                    cmd.Parameters.Add(new MySqlParameter("@CourseId", enrollment.CourseId));
                    cmd.Parameters.Add(new MySqlParameter("@AcademicYearId", enrollment.AcademicYearId));
                    cmd.Parameters.Add(new MySqlParameter("@YearLevelId", enrollment.YearLevelId));
                    cmd.Parameters.Add(new MySqlParameter("@SemesterId", enrollment.SemesterId));
                    cmd.Parameters.Add(new MySqlParameter("@SectionId", enrollment.SectionId));
                    cmd.Parameters.Add(new MySqlParameter("@CurriculumId", (object)enrollment.CurriculumId ?? DBNull.Value));
                    cmd.Parameters.Add(new MySqlParameter("@StudentType", NormalizeStudentType(enrollment.StudentType)));
                    cmd.Parameters.Add(new MySqlParameter("@ScholarshipStatus", (object)NormalizeScholarship(enrollment.ScholarshipStatus) ?? DBNull.Value));
                    cmd.Parameters.Add(new MySqlParameter("@EnrollDate", enrollment.EnrollDate.Date));
                    cmd.Parameters.Add(new MySqlParameter("@TotalUnits", enrollment.TotalUnits));
                    cmd.Parameters.Add(new MySqlParameter("@Status", (object)enrollment.Status ?? "Posted"));

                    cmd.ExecuteNonQuery();
                    enrollmentId = Convert.ToInt32(cmd.LastInsertedId);
                }

                const string sqlDetails = @"
INSERT INTO enrollmentdetails (EnrollmentId, SubjectId, Units, ClassScheduleId, Grade, CreatedAt)
VALUES (@EnrollmentId, @SubjectId, @Units, @ClassScheduleId, @Grade, UTC_TIMESTAMP());";

                foreach (var d in details)
                {
                    using (var cmd = new MySqlCommand(sqlDetails, conn, tx))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Add(new MySqlParameter("@EnrollmentId", enrollmentId));
                        cmd.Parameters.Add(new MySqlParameter("@SubjectId", d.SubjectId));
                        cmd.Parameters.Add(new MySqlParameter("@Units", d.Units));
                        cmd.Parameters.Add(new MySqlParameter("@ClassScheduleId", (object)d.ClassScheduleId ?? DBNull.Value));
                        cmd.Parameters.Add(new MySqlParameter("@Grade", (object)d.Grade ?? DBNull.Value));
                        cmd.ExecuteNonQuery();
                    }
                }

                const string refreshCountsSql = @"
UPDATE subject s
SET s.CurrentEnrolledCount = (
    SELECT COUNT(DISTINCT e.StudentId)
    FROM enrollmentdetails ed
    INNER JOIN enrollment e ON e.EnrollmentId = ed.EnrollmentId
    WHERE ed.SubjectId = s.SubjectId
      AND e.Status <> 'Cancelled'
);";

                using (var cmd = new MySqlCommand(refreshCountsSql, conn, tx))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            });

            return enrollmentId;
        }

        public DataTable GetPrerequisiteFailures(int studentId, IEnumerable<int> subjectIds)
        {
            var ids = BuildDistinctIdList(subjectIds);
            if (ids.Count == 0)
            {
                return new DataTable();
            }

            var parameterNames = BuildParameterNames(ids.Count, "@SubjectId");
            var sql = @"
SELECT
    s.SubjectId,
    s.SubjectCode,
    s.SubjectName,
    pre.SubjectId AS PrerequisiteSubjectId,
    pre.SubjectCode AS PrerequisiteCode,
    pre.SubjectName AS PrerequisiteName
FROM subjectprerequisite sp
INNER JOIN subject s ON s.SubjectId = sp.SubjectId
INNER JOIN subject pre ON pre.SubjectId = sp.PrerequisiteSubjectId
WHERE sp.SubjectId IN (" + string.Join(",", parameterNames) + @")
  AND NOT EXISTS (
      SELECT 1
      FROM enrollment e
      INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
      WHERE e.StudentId = @StudentId
        AND e.Status <> 'Cancelled'
        AND ed.SubjectId = sp.PrerequisiteSubjectId
        AND ed.Grade IS NOT NULL
        AND ed.Grade <= 3.00
  )
ORDER BY s.SubjectCode, pre.SubjectCode;";

            var parameters = new List<MySqlParameter> { new MySqlParameter("@StudentId", studentId) };
            AddIdParameters(parameters, parameterNames, ids);

            return _db.ExecuteDataTable(sql, CommandType.Text, parameters.ToArray());
        }

        public DataTable GetCapacityFailures(IEnumerable<int> subjectIds)
        {
            var ids = BuildDistinctIdList(subjectIds);
            if (ids.Count == 0)
            {
                return new DataTable();
            }

            var parameterNames = BuildParameterNames(ids.Count, "@SubjectId");
            var sql = @"
SELECT
    SubjectId,
    SubjectCode,
    SubjectName,
    MaxStudents,
    CurrentEnrolledCount
FROM subject
WHERE SubjectId IN (" + string.Join(",", parameterNames) + @")
  AND MaxStudents IS NOT NULL
  AND MaxStudents > 0
  AND CurrentEnrolledCount >= MaxStudents
ORDER BY SubjectCode;";

            var parameters = new List<MySqlParameter>();
            AddIdParameters(parameters, parameterNames, ids);

            return _db.ExecuteDataTable(sql, CommandType.Text, parameters.ToArray());
        }

        public DataTable GetScheduleConflicts(int studentId, IEnumerable<int> classScheduleIds, int academicYearId, int semesterId)
        {
            var ids = BuildDistinctIdList(classScheduleIds);
            if (ids.Count == 0)
            {
                return new DataTable();
            }

            var parameterNames = BuildParameterNames(ids.Count, "@ClassScheduleId");
            var sql = @"
SELECT
    wanted.ClassScheduleId,
    wanted.SubjectId,
    wantedSubject.SubjectCode,
    wantedSubject.SubjectName,
    existing.ClassScheduleId AS ExistingClassScheduleId,
    existingSubject.SubjectCode AS ExistingSubjectCode,
    existingSubject.SubjectName AS ExistingSubjectName,
    wanted.DayOfWeek,
    wanted.StartTime,
    wanted.EndTime
FROM classschedule wanted
INNER JOIN subject wantedSubject ON wantedSubject.SubjectId = wanted.SubjectId
INNER JOIN classschedule existing
    ON existing.IsActive = 1
   AND existing.DayOfWeek = wanted.DayOfWeek
   AND existing.StartTime < wanted.EndTime
   AND existing.EndTime > wanted.StartTime
INNER JOIN subject existingSubject ON existingSubject.SubjectId = existing.SubjectId
INNER JOIN enrollmentdetails ed ON ed.ClassScheduleId = existing.ClassScheduleId
INNER JOIN enrollment e ON e.EnrollmentId = ed.EnrollmentId
WHERE wanted.ClassScheduleId IN (" + string.Join(",", parameterNames) + @")
  AND e.StudentId = @StudentId
  AND e.AcademicYearId = @AcademicYearId
  AND e.SemesterId = @SemesterId
  AND e.Status <> 'Cancelled'
ORDER BY wantedSubject.SubjectCode;";

            var parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@StudentId", studentId),
                new MySqlParameter("@AcademicYearId", academicYearId),
                new MySqlParameter("@SemesterId", semesterId)
            };
            AddIdParameters(parameters, parameterNames, ids);

            return _db.ExecuteDataTable(sql, CommandType.Text, parameters.ToArray());
        }

        public bool IsStudentAlreadyEnrolledInSubject(int studentId, int subjectId, int academicYearId, int semesterId)
        {
            const string sql = @"
SELECT COUNT(1)
FROM enrollment e
INNER JOIN enrollmentdetails ed ON ed.EnrollmentId = e.EnrollmentId
WHERE e.StudentId = @StudentId
  AND ed.SubjectId = @SubjectId
  AND e.AcademicYearId = @AcademicYearId
  AND e.SemesterId = @SemesterId
  AND e.Status <> 'Cancelled';";

            var count = Convert.ToInt32(
                _db.ExecuteScalar(
                    sql,
                    CommandType.Text,
                    new[]
                    {
                        new MySqlParameter("@StudentId", studentId),
                        new MySqlParameter("@SubjectId", subjectId),
                        new MySqlParameter("@AcademicYearId", academicYearId),
                        new MySqlParameter("@SemesterId", semesterId)
                    }));

            return count > 0;
        }

        private static List<int> BuildDistinctIdList(IEnumerable<int> ids)
        {
            var result = new List<int>();
            if (ids == null)
            {
                return result;
            }

            foreach (var id in ids)
            {
                if (id <= 0 || result.Contains(id))
                {
                    continue;
                }

                result.Add(id);
            }

            return result;
        }

        private static string[] BuildParameterNames(int count, string prefix)
        {
            var result = new string[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = prefix + i;
            }

            return result;
        }

        private static void AddIdParameters(ICollection<MySqlParameter> parameters, string[] parameterNames, IList<int> ids)
        {
            for (var i = 0; i < ids.Count; i++)
            {
                parameters.Add(new MySqlParameter(parameterNames[i], ids[i]));
            }
        }

        private static string NormalizeStudentType(string value)
        {
            if (string.Equals(value, "Irregular", StringComparison.OrdinalIgnoreCase))
            {
                return "Irregular";
            }

            if (string.Equals(value, "Summer", StringComparison.OrdinalIgnoreCase))
            {
                return "Summer";
            }

            return "Regular";
        }

        private static string NormalizeScholarship(string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.Equals(value, "None", StringComparison.OrdinalIgnoreCase)
                ? null
                : value.Trim();
        }
    }
}

