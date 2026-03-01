-- Populate ClassSchedule for active sections (focus on BSIT/BSEd/BSBA/BSCrim/BSN sections)
-- Uses existing Subject/Section/Faculty records in app schema.

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
SELECT
    p.SectionId,
    p.SubjectId,
    CASE p.CourseId
        WHEN 2 THEN 7  -- BSIT
        WHEN 4 THEN 3  -- BSED
        WHEN 5 THEN 4  -- BSBA
        WHEN 6 THEN 5  -- BSCRIM
        WHEN 7 THEN 6  -- BSN
        ELSE 1
    END AS FacultyId,
    CASE p.rn
        WHEN 1 THEN 'Mon/Wed'
        WHEN 2 THEN 'Tue/Thu'
        WHEN 3 THEN 'Mon'
        WHEN 4 THEN 'Tue'
        WHEN 5 THEN 'Fri'
    END AS DayOfWeek,
    CASE p.rn
        WHEN 1 THEN '08:00:00'
        WHEN 2 THEN '09:30:00'
        WHEN 3 THEN '13:00:00'
        WHEN 4 THEN '14:30:00'
        WHEN 5 THEN '16:00:00'
    END AS StartTime,
    CASE p.rn
        WHEN 1 THEN '09:30:00'
        WHEN 2 THEN '11:00:00'
        WHEN 3 THEN '14:30:00'
        WHEN 4 THEN '16:00:00'
        WHEN 5 THEN '17:30:00'
    END AS EndTime,
    CONCAT('R-', LPAD((p.SectionId * 10) + p.rn, 3, '0')) AS Room,
    CONCAT('Seeded schedule for ', p.SectionName) AS Remarks,
    p.AcademicYearId,
    p.SemesterId,
    1,
    UTC_TIMESTAMP()
FROM
(
    SELECT
        sec.SectionId,
        sec.SectionName,
        sec.CourseId,
        sec.AcademicYearId,
        sec.SemesterId,
        rs.SubjectId,
        rs.rn
    FROM section sec
    INNER JOIN course c ON c.CourseId = sec.CourseId
    INNER JOIN
    (
        SELECT ranked.SubjectId, ranked.CourseId, ranked.rn
        FROM
        (
            SELECT
                s.SubjectId,
                s.CourseId,
                ROW_NUMBER() OVER (PARTITION BY s.CourseId ORDER BY s.SubjectName, s.SubjectId) AS rn
            FROM subject s
            WHERE s.IsActive = 1
        ) ranked
        WHERE ranked.rn <= 5
    ) rs ON rs.CourseId = sec.CourseId
    WHERE sec.IsActive = 1
      AND c.CourseCode IN ('BSIT', 'BSED', 'BSBA', 'BSCRIM', 'BSN')
) p
WHERE NOT EXISTS (
    SELECT 1
    FROM classschedule cs
    WHERE cs.SectionId = p.SectionId
      AND cs.SubjectId = p.SubjectId
      AND cs.AcademicYearId = p.AcademicYearId
      AND cs.SemesterId = p.SemesterId
);
