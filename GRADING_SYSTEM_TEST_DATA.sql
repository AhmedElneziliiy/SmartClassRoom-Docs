-- ============================================
-- Grading System Test Data Setup Script
-- ============================================
-- This script helps verify required data exists
-- Run these queries to check your current data state

-- ============================================
-- 1. CHECK EXISTING DATA
-- ============================================

PRINT '=== CHECKING USERS ==='
SELECT
    Id,
    Name,
    Email,
    CASE
        WHEN Id IN (SELECT UserId FROM AspNetUserRoles WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')) THEN 'Admin'
        WHEN Id IN (SELECT UserId FROM AspNetUserRoles WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Teacher')) THEN 'Teacher'
        WHEN Id IN (SELECT UserId FROM AspNetUserRoles WHERE RoleId IN (SELECT Id FROM AspNetRoles WHERE Name = 'Student')) THEN 'Student'
        ELSE 'No Role'
    END AS UserRole
FROM Users
ORDER BY Email;

PRINT '=== CHECKING TERMS ==='
SELECT
    Id,
    Name,
    StartDate,
    EndDate,
    Status,
    CASE
        WHEN GETDATE() BETWEEN StartDate AND EndDate THEN 'Current Term'
        WHEN GETDATE() < StartDate THEN 'Future Term'
        ELSE 'Past Term'
    END AS TimeStatus
FROM Terms
ORDER BY StartDate DESC;

PRINT '=== CHECKING DEPARTMENTS ==='
SELECT Id, Name, Code FROM Departments;

PRINT '=== CHECKING LEVELS ==='
SELECT Id, Name FROM Levels;

PRINT '=== CHECKING COURSES ==='
SELECT
    Id,
    Code,
    Name,
    Credits,
    (SELECT Name FROM Departments WHERE Id = Courses.DepartmentId) AS Department
FROM Courses;

PRINT '=== CHECKING COURSE OFFERINGS ==='
SELECT
    co.Id,
    c.Code AS CourseCode,
    c.Name AS CourseName,
    t.Name AS Term,
    u.Name AS Teacher,
    co.MaxStudents,
    co.Status,
    (SELECT COUNT(*) FROM StudentEnrollments WHERE CourseOfferingId = co.Id AND Status = 'Enrolled') AS EnrolledCount,
    (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) AS ComponentCount,
    (SELECT SUM(Weight) FROM GradeComponents WHERE CourseOfferingId = co.Id) AS TotalWeight
FROM CourseOfferings co
JOIN Courses c ON co.CourseId = c.Id
JOIN Terms t ON co.TermId = t.Id
JOIN Users u ON co.TeacherId = u.Id
ORDER BY t.StartDate DESC, c.Code;

PRINT '=== CHECKING STUDENT ENROLLMENTS ==='
SELECT
    co.Id AS OfferingId,
    c.Code AS CourseCode,
    s.Name AS StudentName,
    se.Status AS EnrollmentStatus,
    se.FinalGrade,
    se.LetterGrade
FROM StudentEnrollments se
JOIN CourseOfferings co ON se.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
JOIN Users s ON se.StudentId = s.Id
ORDER BY co.Id, s.Name;

PRINT '=== CHECKING GRADE COMPONENTS ==='
SELECT
    gc.Id,
    c.Code AS CourseCode,
    gc.ComponentName,
    gc.Weight,
    gc.MaxScore,
    gc.OrderIndex,
    (SELECT COUNT(*) FROM Grades WHERE GradeComponentId = gc.Id) AS GradeCount
FROM GradeComponents gc
JOIN CourseOfferings co ON gc.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
ORDER BY co.Id, gc.OrderIndex;

PRINT '=== CHECKING GRADES ==='
SELECT
    c.Code AS CourseCode,
    gc.ComponentName,
    s.Name AS StudentName,
    g.Score,
    gc.MaxScore,
    CAST((g.Score / gc.MaxScore * 100) AS DECIMAL(5,2)) AS Percentage
FROM Grades g
JOIN GradeComponents gc ON g.GradeComponentId = gc.Id
JOIN CourseOfferings co ON g.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
JOIN Users s ON g.StudentId = s.Id
ORDER BY co.Id, s.Name, gc.OrderIndex;

-- ============================================
-- 2. SAMPLE TEST DATA (Optional - only if needed)
-- ============================================
-- Uncomment and modify as needed

/*
-- Create a test Term (if needed)
INSERT INTO Terms (Name, StartDate, EndDate, Status, CreatedAt)
VALUES ('Spring 2024', '2024-01-15', '2024-05-31', 'Active', GETDATE());

-- Create test Course (if needed)
DECLARE @DeptId INT = (SELECT TOP 1 Id FROM Departments);
INSERT INTO Courses (Code, Name, Credits, DepartmentId, CreatedAt)
VALUES ('TEST101', 'Test Course for Grading', 3, @DeptId, GETDATE());

-- Create Course Offering (if needed)
DECLARE @CourseId INT = (SELECT Id FROM Courses WHERE Code = 'TEST101');
DECLARE @TermId INT = (SELECT Id FROM Terms WHERE Name = 'Spring 2024');
DECLARE @TeacherId INT = (SELECT TOP 1 Id FROM Users u
    JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name = 'Teacher');

INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, MaxStudents, SessionsPerWeek, Status, CreatedAt)
VALUES (@CourseId, @TermId, @TeacherId, 30, 2, 'Active', GETDATE());

-- Enroll test students (if needed)
DECLARE @OfferingId INT = SCOPE_IDENTITY();

INSERT INTO StudentEnrollments (StudentId, CourseOfferingId, EnrollmentDate, Status)
SELECT TOP 5
    u.Id,
    @OfferingId,
    GETDATE(),
    'Enrolled'
FROM Users u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Student'
AND u.Id NOT IN (SELECT StudentId FROM StudentEnrollments WHERE CourseOfferingId = @OfferingId);
*/

-- ============================================
-- 3. DATA VALIDATION CHECKS
-- ============================================

PRINT '=== VALIDATION CHECKS ==='

-- Check 1: Active Terms
DECLARE @ActiveTermCount INT = (SELECT COUNT(*) FROM Terms WHERE Status = 'Active');
PRINT 'Active Terms: ' + CAST(@ActiveTermCount AS VARCHAR);
IF @ActiveTermCount = 0
    PRINT '❌ WARNING: No active terms found!';
ELSE
    PRINT '✅ Active terms exist';

-- Check 2: Active Course Offerings
DECLARE @ActiveOfferingCount INT = (SELECT COUNT(*) FROM CourseOfferings WHERE Status = 'Active');
PRINT 'Active Course Offerings: ' + CAST(@ActiveOfferingCount AS VARCHAR);
IF @ActiveOfferingCount = 0
    PRINT '❌ WARNING: No active course offerings found!';
ELSE
    PRINT '✅ Active course offerings exist';

-- Check 3: Enrolled Students
DECLARE @EnrolledStudentCount INT = (SELECT COUNT(DISTINCT StudentId) FROM StudentEnrollments WHERE Status = 'Enrolled');
PRINT 'Enrolled Students: ' + CAST(@EnrolledStudentCount AS VARCHAR);
IF @EnrolledStudentCount < 3
    PRINT '❌ WARNING: Less than 3 students enrolled!';
ELSE
    PRINT '✅ Sufficient students enrolled';

-- Check 4: Offerings with Components
PRINT '';
PRINT '=== OFFERINGS COMPONENT STATUS ==='
SELECT
    co.Id AS OfferingId,
    c.Code AS CourseCode,
    c.Name AS CourseName,
    (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) AS ComponentCount,
    COALESCE((SELECT SUM(Weight) FROM GradeComponents WHERE CourseOfferingId = co.Id), 0) AS TotalWeight,
    CASE
        WHEN (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) = 0 THEN '❌ Not Configured'
        WHEN (SELECT SUM(Weight) FROM GradeComponents WHERE CourseOfferingId = co.Id) <> 100 THEN '⚠️ Invalid Weight'
        ELSE '✅ Ready for Grading'
    END AS Status
FROM CourseOfferings co
JOIN Courses c ON co.CourseId = c.Id
WHERE co.Status = 'Active';

-- Check 5: Students Missing Grades
PRINT '';
PRINT '=== STUDENTS WITH INCOMPLETE GRADES ==='
SELECT
    co.Id AS OfferingId,
    c.Code AS CourseCode,
    s.Name AS StudentName,
    (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) AS TotalComponents,
    (SELECT COUNT(*) FROM Grades g WHERE g.StudentId = se.StudentId AND g.CourseOfferingId = co.Id) AS GradesEntered,
    CASE
        WHEN (SELECT COUNT(*) FROM GradeComponents WHERE CourseOfferingId = co.Id) =
             (SELECT COUNT(*) FROM Grades g WHERE g.StudentId = se.StudentId AND g.CourseOfferingId = co.Id)
        THEN '✅ Complete'
        ELSE '❌ Incomplete'
    END AS GradeStatus
FROM StudentEnrollments se
JOIN CourseOfferings co ON se.CourseOfferingId = co.Id
JOIN Courses c ON co.CourseId = c.Id
JOIN Users s ON se.StudentId = s.Id
WHERE se.Status = 'Enrolled'
AND co.Status = 'Active'
ORDER BY co.Id, s.Name;

-- ============================================
-- 4. CLEANUP QUERIES (Use with caution!)
-- ============================================

/*
-- Delete all grades for a specific offering (CAUTION!)
-- DECLARE @OfferingIdToClean INT = 1;
-- DELETE FROM Grades WHERE CourseOfferingId = @OfferingIdToClean;

-- Delete all grade components for a specific offering (CAUTION!)
-- DELETE FROM GradeComponents WHERE CourseOfferingId = @OfferingIdToClean;

-- Reset final grades for all enrollments (CAUTION!)
-- UPDATE StudentEnrollments
-- SET FinalGrade = NULL, LetterGrade = NULL, Status = 'Enrolled'
-- WHERE CourseOfferingId = @OfferingIdToClean;

-- Reset all student GPAs (CAUTION!)
-- UPDATE Users SET CurrentGPA = 0 WHERE Id IN (
--     SELECT UserId FROM AspNetUserRoles WHERE RoleId IN (
--         SELECT Id FROM AspNetRoles WHERE Name = 'Student'
--     )
-- );
*/

PRINT '';
PRINT '=== CHECKS COMPLETE ===';
