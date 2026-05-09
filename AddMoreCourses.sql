-- SQL Script to add more courses and course offerings to fill the timetable
-- This will create additional courses for Level 1 CS students

-- Note: You'll need to update the IDs below to match your actual database
-- Check your database for: DepartmentId, LevelId, SectionId, TermId, TeacherId

-- First, let's add more courses (adjust DepartmentId as needed)
DECLARE @DepartmentId INT = (SELECT TOP 1 Id FROM Departments WHERE Name LIKE '%Computer%' OR Name LIKE '%CS%');
DECLARE @TermId INT = (SELECT TOP 1 Id FROM Terms WHERE Name = 'Fall 2024');
DECLARE @SectionId INT = (SELECT TOP 1 Id FROM Sections WHERE IsActive = 1);
DECLARE @TeacherId INT = (SELECT TOP 1 Id FROM Users WHERE UserType = 'Teacher');

-- Insert additional courses if they don't exist
IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'MATH101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Mathematics I', 'MATH101', 3, @DepartmentId, 1, GETUTCDATE(), 'Theory');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'ENG101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('English Communication', 'ENG101', 2, @DepartmentId, 1, GETUTCDATE(), 'Theory');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'PHYS101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Physics I', 'PHYS101', 3, @DepartmentId, 1, GETUTCDATE(), 'Theory');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'ARCH101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Computer Architecture', 'ARCH101', 3, @DepartmentId, 1, GETUTCDATE(), 'Theory');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'DISC101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Discrete Mathematics', 'DISC101', 3, @DepartmentId, 1, GETUTCDATE(), 'Theory');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'WEB101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Web Development', 'WEB101', 3, @DepartmentId, 1, GETUTCDATE(), 'Lab');

IF NOT EXISTS (SELECT 1 FROM Courses WHERE Code = 'DB101')
INSERT INTO Courses (Name, Code, Credits, DepartmentId, IsActive, CreatedAt, CourseType)
VALUES ('Database Fundamentals', 'DB101', 3, @DepartmentId, 1, GETUTCDATE(), 'Lab');

-- Now create Course Offerings for these courses (3 sessions per week each)
DECLARE @MATH101Id INT = (SELECT Id FROM Courses WHERE Code = 'MATH101');
DECLARE @ENG101Id INT = (SELECT Id FROM Courses WHERE Code = 'ENG101');
DECLARE @PHYS101Id INT = (SELECT Id FROM Courses WHERE Code = 'PHYS101');
DECLARE @ARCH101Id INT = (SELECT Id FROM Courses WHERE Code = 'ARCH101');
DECLARE @DISC101Id INT = (SELECT Id FROM Courses WHERE Code = 'DISC101');
DECLARE @WEB101Id INT = (SELECT Id FROM Courses WHERE Code = 'WEB101');
DECLARE @DB101Id INT = (SELECT Id FROM Courses WHERE Code = 'DB101');

-- Mathematics I - 3 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @MATH101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@MATH101Id, @TermId, @TeacherId, @SectionId, 3, 30, 'Active', GETUTCDATE());

-- English Communication - 2 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @ENG101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@ENG101Id, @TermId, @TeacherId, @SectionId, 2, 30, 'Active', GETUTCDATE());

-- Physics I - 3 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @PHYS101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@PHYS101Id, @TermId, @TeacherId, @SectionId, 3, 30, 'Active', GETUTCDATE());

-- Computer Architecture - 3 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @ARCH101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@ARCH101Id, @TermId, @TeacherId, @SectionId, 3, 30, 'Active', GETUTCDATE());

-- Discrete Mathematics - 3 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @DISC101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@DISC101Id, @TermId, @TeacherId, @SectionId, 3, 30, 'Active', GETUTCDATE());

-- Web Development - 2 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @WEB101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@WEB101Id, @TermId, @TeacherId, @SectionId, 2, 30, 'Active', GETUTCDATE());

-- Database Fundamentals - 2 sessions per week
IF NOT EXISTS (SELECT 1 FROM CourseOfferings WHERE CourseId = @DB101Id AND TermId = @TermId AND SectionId = @SectionId)
INSERT INTO CourseOfferings (CourseId, TermId, TeacherId, SectionId, SessionsPerWeek, MaxStudents, Status, CreatedAt)
VALUES (@DB101Id, @TermId, @TeacherId, @SectionId, 2, 30, 'Active', GETUTCDATE());

-- Reduce the sessions per week for existing courses to more realistic values
-- Update Introduction to Programming from 6 to 3
UPDATE CourseOfferings
SET SessionsPerWeek = 3
WHERE CourseId IN (SELECT Id FROM Courses WHERE Name = 'Introduction to Programming')
  AND TermId = @TermId
  AND SectionId = @SectionId;

-- Update Data Structures from 5 to 3
UPDATE CourseOfferings
SET SessionsPerWeek = 3
WHERE CourseId IN (SELECT Id FROM Courses WHERE Name = 'Data Structures')
  AND TermId = @TermId
  AND SectionId = @SectionId;

PRINT 'Courses and Course Offerings added successfully!';
PRINT 'Total course offerings: ~9 courses × ~3 sessions = ~27 slots per week';
PRINT 'This should fill about 60% of your timetable (27 out of 45 available slots)';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Delete your current timetable';
PRINT '2. Generate a new timetable';
PRINT '3. The timetable should now be much more filled!';
