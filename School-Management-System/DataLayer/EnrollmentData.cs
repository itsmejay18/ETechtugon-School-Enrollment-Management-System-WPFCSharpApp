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
            });

            return enrollmentId;
        }
    }
}

