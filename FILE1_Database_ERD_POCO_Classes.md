# FILE 1: DATABASE ERD & POCO CLASSES
## SmartClassRoom-LetUNO - MS SQL Server + Entity Framework Code First

**Version**: 1.0  
**Date**: December 4, 2025  
**Database**: MS SQL Server  
**ORM**: Entity Framework Core 8.0  
**Approach**: Code First with Migrations  
**Authentication**: ASP.NET Core Identity Framework

---

## 1. ENTITY RELATIONSHIP DIAGRAM (ERD)

### 1.1 ERD Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                      IDENTITY FRAMEWORK TABLES                       │
│  (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.)                 │
└─────────────────────────────────────────────────────────────────────┘
                              │
                              │ Inherits
                              ↓
┌────────────────┐         ┌──────────────┐         ┌────────────────┐
│   University   │────────>│ Department   │────────>│     Level      │
│                │   1:N    │              │   1:N   │  (Year 1-4)    │
└────────────────┘         └──────────────┘         └────────────────┘
                                   │                         │
                                   │ 1:N                     │ 1:N
                                   ↓                         ↓
                            ┌──────────────┐         ┌────────────────┐
                            │   Section    │────────>│     Group      │
                            │   (A, B, C)  │   1:N   │                │
                            └──────────────┘         └────────────────┘
                                   │
                                   │ N:M
                                   ↓
┌────────────────┐         ┌──────────────┐         ┌────────────────┐
│     Course     │────────>│CourseOffering│────────>│     Term       │
│                │   1:N    │              │   N:1   │  (Fall 2025)   │
└────────────────┘         └──────────────┘         └────────────────┘
        │                          │                         
        │                          │ N:M                     
        │                          ↓                         
        │                  ┌──────────────┐                 
        │                  │   Student    │ (extends AspNetUsers)
        │                  │ Enrollment   │                 
        │                  └──────────────┘                 
        │                          │
        │                          │ 1:N
        │                          ↓
        │                  ┌──────────────┐         ┌────────────────┐
        │                  │   Session    │────────>│   Attendance   │
        │                  │              │   1:N   │   (Face Auth)  │
        │                  └──────────────┘         └────────────────┘
        │                          │
        │                          │ N:1
        │                          ↓
        │                  ┌──────────────┐
        │                  │     Room     │
        │                  │              │
        │                  └──────────────┘
        │
        │ N:M
        ↓
┌────────────────┐         ┌──────────────┐         ┌────────────────┐
│     Quiz       │────────>│QuizAssignment│────────>│  QuizAttempt   │
│                │   1:N    │              │   1:N   │                │
└────────────────┘         └──────────────┘         └────────────────┘
        │
        │ 1:N
        ↓
┌────────────────┐
│ QuizQuestion   │
│                │
└────────────────┘

┌────────────────┐         ┌──────────────┐         ┌────────────────┐
│GradeComponent  │────────>│     Grade    │────────>│CourseOffering  │
│(Midterm 30%)   │   1:N    │              │   N:1   │                │
└────────────────┘         └──────────────┘         └────────────────┘

┌────────────────┐         ┌──────────────┐
│   Timetable    │────────>│ScheduledSlot │
│                │   1:N    │              │
└────────────────┘         └──────────────┘

┌────────────────┐         ┌──────────────┐
│   Material     │────────>│MaterialFolder│
│                │   N:1    │              │
└────────────────┘         └──────────────┘

┌────────────────┐
│   ESPDevice    │
│  (ESP32-CAM)   │
└────────────────┘
```

---

## 2. IDENTITY FRAMEWORK INTEGRATION

### 2.1 Custom User Entity (extends IdentityUser)

**POCO Class: ApplicationUser.cs**

```csharp
/// <summary>
/// Custom user entity extending IdentityUser for Identity Framework
/// Base for all user types (Students, Teachers, Admins)
/// </summary>
public class ApplicationUser : IdentityUser<int> // Using int as primary key
{
    // Identity Framework provides:
    // - Id (int)
    // - UserName
    // - Email
    // - EmailConfirmed
    // - PasswordHash
    // - SecurityStamp
    // - PhoneNumber
    // - TwoFactorEnabled
    // - LockoutEnd
    // - LockoutEnabled
    // - AccessFailedCount

    // Custom Properties
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [MaxLength(20)]
    public string? NationalId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; } // Male/Female

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(255)]
    public string? ProfilePhotoPath { get; set; }

    // Face Recognition - Store Base64 Embedding
    public string? FaceEmbeddingBase64 { get; set; } // For your custom AI model

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public int? UniversityId { get; set; }

    // Navigation Properties
    public virtual University? University { get; set; }

    // Discriminator for inheritance (TPH - Table Per Hierarchy)
    [MaxLength(50)]
    public string UserType { get; set; } // "Student", "Teacher", "Admin"
}
```

### 2.2 Student Entity (extends ApplicationUser)

**POCO Class: Student.cs**

```csharp
/// <summary>
/// Student entity - inherits from ApplicationUser
/// </summary>
public class Student : ApplicationUser
{
    [MaxLength(20)]
    public string? StudentCode { get; set; }

    public int? DepartmentId { get; set; }
    public int? LevelId { get; set; }
    public int? SectionId { get; set; }
    public int? GroupId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    [MaxLength(20)]
    public string? AcademicStatus { get; set; } // Active, Suspended, Graduated

    public decimal? CurrentGPA { get; set; }

    // Navigation Properties
    public virtual Department? Department { get; set; }
    public virtual Level? Level { get; set; }
    public virtual Section? Section { get; set; }
    public virtual Group? Group { get; set; }

    // Collections
    public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
```

### 2.3 Teacher Entity (extends ApplicationUser)

**POCO Class: Teacher.cs**

```csharp
/// <summary>
/// Teacher entity - inherits from ApplicationUser
/// </summary>
public class Teacher : ApplicationUser
{
    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    public int? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; } // Prof., Dr., Mr., Ms.

    [MaxLength(100)]
    public string? Specialization { get; set; }

    public DateTime? HireDate { get; set; }

    [MaxLength(20)]
    public string? EmploymentStatus { get; set; } // Full-time, Part-time, Adjunct

    // Navigation Properties
    public virtual Department? Department { get; set; }

    // Collections
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();
}
```

### 2.4 Custom Role Entity (extends IdentityRole)

**POCO Class: ApplicationRole.cs**

```csharp
/// <summary>
/// Custom role entity for granular permissions
/// </summary>
public class ApplicationRole : IdentityRole<int>
{
    // Identity Framework provides:
    // - Id (int)
    // - Name
    // - NormalizedName
    // - ConcurrencyStamp

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
}
```

### 2.5 Custom UserRole (for many-to-many relationship)

**POCO Class: ApplicationUserRole.cs**

```csharp
/// <summary>
/// Junction table for User-Role relationship
/// </summary>
public class ApplicationUserRole : IdentityUserRole<int>
{
    // Identity Framework provides:
    // - UserId (int)
    // - RoleId (int)

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ApplicationUser User { get; set; }
    public virtual ApplicationRole Role { get; set; }
}
```

---

## 3. ACADEMIC STRUCTURE ENTITIES

### 3.1 University

**POCO Class: University.cs**

```csharp
/// <summary>
/// University/Institution entity
/// </summary>
public class University
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(255)]
    public string? LogoPath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
```

### 3.2 Department

**POCO Class: Department.cs**

```csharp
/// <summary>
/// Academic department entity
/// </summary>
public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(20)]
    public string? Code { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int UniversityId { get; set; }

    public int? HeadOfDepartmentId { get; set; } // Foreign key to Teacher

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual University University { get; set; }
    public virtual Teacher? HeadOfDepartment { get; set; }

    // Collections
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    public virtual ICollection<Level> Levels { get; set; } = new List<Level>();
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
```

### 3.3 Level (Year Level)

**POCO Class: Level.cs**

```csharp
/// <summary>
/// Academic level/year entity (1st Year, 2nd Year, etc.)
/// </summary>
public class Level
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } // "Level 1", "Year 1"

    public int LevelNumber { get; set; } // 1, 2, 3, 4

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Department Department { get; set; }

    // Collections
    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
```

### 3.4 Section

**POCO Class: Section.cs**

```csharp
/// <summary>
/// Class section entity (Section A, Section B, etc.)
/// </summary>
public class Section
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } // "Section A"

    [MaxLength(10)]
    public string? Code { get; set; } // "A", "B", "C"

    public int LevelId { get; set; }

    public int? Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Level Level { get; set; }

    // Collections
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
}
```

### 3.5 Group

**POCO Class: Group.cs**

```csharp
/// <summary>
/// Student group within a section (for lab sessions, projects)
/// </summary>
public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } // "Group 1"

    [MaxLength(10)]
    public string? Code { get; set; } // "G1", "G2"

    public int SectionId { get; set; }

    public int? Capacity { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Section Section { get; set; }

    // Collections
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
```

---

## 4. COURSE MANAGEMENT ENTITIES

### 4.1 Course

**POCO Class: Course.cs**

```csharp
/// <summary>
/// Course catalog entity
/// </summary>
public class Course
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } // CS101, MATH201

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Credits { get; set; }

    [MaxLength(50)]
    public string? CourseType { get; set; } // Lecture, Lab, Hybrid

    public int DepartmentId { get; set; }

    public int? PrerequisiteCourseId { get; set; } // Self-referencing FK

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Department Department { get; set; }
    public virtual Course? PrerequisiteCourse { get; set; }

    // Collections
    public virtual ICollection<CourseOffering> Offerings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<Course> PrerequisiteFor { get; set; } = new List<Course>();
}
```

### 4.2 Term (Semester)

**POCO Class: Term.cs**

```csharp
/// <summary>
/// Academic term/semester entity
/// </summary>
public class Term
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } // "Fall 2025", "Spring 2026"

    [MaxLength(20)]
    public string? Code { get; set; } // "F2025"

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Upcoming, Active, Completed

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Collections
    public virtual ICollection<CourseOffering> CourseOfferings { get; set; } = new List<CourseOffering>();
    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
```

### 4.3 CourseOffering

**POCO Class: CourseOffering.cs**

```csharp
/// <summary>
/// Course offering entity (specific instance of a course in a term)
/// </summary>
public class CourseOffering
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }
    public int TermId { get; set; }
    public int TeacherId { get; set; }
    public int? SectionId { get; set; }

    public int SessionsPerWeek { get; set; } = 2;

    public int MaxStudents { get; set; } = 50;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Active, Completed, Cancelled

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Course Course { get; set; }
    public virtual Term Term { get; set; }
    public virtual Teacher Teacher { get; set; }
    public virtual Section? Section { get; set; }

    // Collections
    public virtual ICollection<StudentEnrollment> Enrollments { get; set; } = new List<StudentEnrollment>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<QuizAssignment> QuizAssignments { get; set; } = new List<QuizAssignment>();
    public virtual ICollection<GradeComponent> GradeComponents { get; set; } = new List<GradeComponent>();
    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
```

### 4.4 StudentEnrollment

**POCO Class: StudentEnrollment.cs**

```csharp
/// <summary>
/// Student enrollment in course offering (junction table)
/// </summary>
public class StudentEnrollment
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public int CourseOfferingId { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string? Status { get; set; } // Enrolled, Dropped, Completed

    public decimal? FinalGrade { get; set; }

    [MaxLength(5)]
    public string? LetterGrade { get; set; } // A, B+, B, C+, etc.

    // Navigation Properties
    public virtual Student Student { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }
}
```

---

## 5. SCHEDULING ENTITIES

### 5.1 Timetable

**POCO Class: Timetable.cs**

```csharp
/// <summary>
/// Generated timetable entity
/// </summary>
public class Timetable
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    public int DepartmentId { get; set; }
    public int LevelId { get; set; }
    public int? SectionId { get; set; }
    public int TermId { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Draft, Published

    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? PublishedBy { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Navigation Properties
    public virtual Department Department { get; set; }
    public virtual Level Level { get; set; }
    public virtual Section? Section { get; set; }
    public virtual Term Term { get; set; }
    public virtual ApplicationUser? Creator { get; set; }

    // Collections
    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
}
```

### 5.2 ScheduledSlot

**POCO Class: ScheduledSlot.cs**

```csharp
/// <summary>
/// Scheduled time slot in timetable
/// </summary>
public class ScheduledSlot
{
    [Key]
    public int Id { get; set; }

    public int TimetableId { get; set; }
    public int CourseOfferingId { get; set; }

    [Required]
    [MaxLength(20)]
    public string DayOfWeek { get; set; } // Monday, Tuesday, etc.

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public int? RoomId { get; set; }

    // Navigation Properties
    public virtual Timetable Timetable { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }
    public virtual Room? Room { get; set; }
}
```

### 5.3 Session

**POCO Class: Session.cs**

```csharp
/// <summary>
/// Actual class session entity
/// </summary>
public class Session
{
    [Key]
    public int Id { get; set; }

    public int CourseOfferingId { get; set; }

    public DateTime SessionDate { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int? RoomId { get; set; }

    [MaxLength(50)]
    public string? SessionType { get; set; } // Regular, Makeup, Exam

    [MaxLength(500)]
    public string? Topic { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Scheduled, InProgress, Completed, Cancelled

    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public int? TimetableId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual CourseOffering CourseOffering { get; set; }
    public virtual Room? Room { get; set; }
    public virtual Timetable? Timetable { get; set; }

    // Collections
    public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
}
```

### 5.4 Room

**POCO Class: Room.cs**

```csharp
/// <summary>
/// Classroom/Room entity
/// </summary>
public class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } // "101", "A-205"

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? RoomType { get; set; } // Lecture, Lab, Auditorium

    public int Capacity { get; set; }

    [MaxLength(100)]
    public string? Building { get; set; }

    [MaxLength(10)]
    public string? Floor { get; set; }

    [MaxLength(500)]
    public string? Equipment { get; set; } // Projector, Whiteboard, Computers

    [MaxLength(20)]
    public string? Status { get; set; } // Available, Maintenance, Reserved

    public bool IsActive { get; set; } = true;

    // Collections
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<ScheduledSlot> ScheduledSlots { get; set; } = new List<ScheduledSlot>();
}
```

---

## 6. ATTENDANCE ENTITIES

### 6.1 Attendance

**POCO Class: Attendance.cs**

```csharp
/// <summary>
/// Student attendance record
/// </summary>
public class Attendance
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }
    public int StudentId { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    public DateTime? CheckOutTime { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Present, Absent, Late, Excused

    [MaxLength(50)]
    public string? VerificationMethod { get; set; } // Face, Manual, ESP32

    // Location (GPS)
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int? MarkedBy { get; set; } // Teacher ID if manual

    [MaxLength(500)]
    public string? Notes { get; set; }

    public int? ESPDeviceId { get; set; }

    // Navigation Properties
    public virtual Session Session { get; set; }
    public virtual Student Student { get; set; }
    public virtual Teacher? MarkedByTeacher { get; set; }
    public virtual ESPDevice? ESPDevice { get; set; }
}
```

### 6.2 ESPDevice

**POCO Class: ESPDevice.cs**

```csharp
/// <summary>
/// ESP32-CAM device entity
/// </summary>
public class ESPDevice
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string DeviceId { get; set; } // Unique device identifier

    [MaxLength(100)]
    public string? DeviceName { get; set; }

    public int? RoomId { get; set; }

    [MaxLength(20)]
    public string? MacAddress { get; set; }

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // Active, Inactive, Maintenance

    public DateTime? LastSeen { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Room? Room { get; set; }

    // Collections
    public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
}
```

---

## 7. QUIZ ENTITIES

### 7.1 Quiz

**POCO Class: Quiz.cs**

```csharp
/// <summary>
/// Quiz entity
/// </summary>
public class Quiz
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int CourseId { get; set; }

    public int TimeLimit { get; set; } // in minutes

    public decimal PassingScore { get; set; } // percentage

    public int? MaxAttempts { get; set; }

    public bool ShuffleQuestions { get; set; } = false;
    public bool ShowCorrectAnswers { get; set; } = true;

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Course Course { get; set; }
    public virtual Teacher Creator { get; set; }

    // Collections
    public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public virtual ICollection<QuizAssignment> Assignments { get; set; } = new List<QuizAssignment>();
}
```

### 7.2 QuizQuestion

**POCO Class: QuizQuestion.cs**

```csharp
/// <summary>
/// Quiz question entity (multiple-choice)
/// </summary>
public class QuizQuestion
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }

    [Required]
    public string QuestionText { get; set; }

    [Required]
    public string Option1 { get; set; }

    [Required]
    public string Option2 { get; set; }

    [Required]
    public string Option3 { get; set; }

    [Required]
    public string Option4 { get; set; }

    public int CorrectAnswer { get; set; } // 1, 2, 3, or 4

    public decimal Points { get; set; } = 1.0m;

    public int OrderIndex { get; set; }

    [MaxLength(1000)]
    public string? Explanation { get; set; }

    // Navigation Properties
    public virtual Quiz Quiz { get; set; }
}
```

### 7.3 QuizAssignment

**POCO Class: QuizAssignment.cs**

```csharp
/// <summary>
/// Quiz assignment to course offering
/// </summary>
public class QuizAssignment
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }
    public int CourseOfferingId { get; set; }

    public DateTime AvailableFrom { get; set; }
    public DateTime AvailableUntil { get; set; }

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Quiz Quiz { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    // Collections
    public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
```

### 7.4 QuizAttempt

**POCO Class: QuizAttempt.cs**

```csharp
/// <summary>
/// Student quiz attempt entity
/// </summary>
public class QuizAttempt
{
    [Key]
    public int Id { get; set; }

    public int QuizAssignmentId { get; set; }
    public int StudentId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    // Store answers as JSON: { "questionId": selectedOption }
    public string? Answers { get; set; }

    // Store detailed results as JSON
    public string? Results { get; set; }

    public decimal? Score { get; set; }
    public decimal? TotalPoints { get; set; }
    public decimal? Percentage { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; } // InProgress, Submitted, Graded

    public int AttemptNumber { get; set; } = 1;

    // Navigation Properties
    public virtual QuizAssignment QuizAssignment { get; set; }
    public virtual Student Student { get; set; }
}
```

---

## 8. GRADING ENTITIES

### 8.1 GradeComponent

**POCO Class: GradeComponent.cs**

```csharp
/// <summary>
/// Grade component configuration (Midterm, Final, Assignment, etc.)
/// </summary>
public class GradeComponent
{
    [Key]
    public int Id { get; set; }

    public int CourseOfferingId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ComponentName { get; set; } // Midterm, Final, Assignment

    [Required]
    public decimal Weight { get; set; } // Percentage (0-100)

    public decimal MaxScore { get; set; } = 100;

    public int OrderIndex { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation Properties
    public virtual CourseOffering CourseOffering { get; set; }

    // Collections
    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
```

### 8.2 Grade

**POCO Class: Grade.cs**

```csharp
/// <summary>
/// Student grade entity
/// </summary>
public class Grade
{
    [Key]
    public int Id { get; set; }

    public int StudentId { get; set; }
    public int CourseOfferingId { get; set; }
    public int GradeComponentId { get; set; }

    public decimal Score { get; set; }

    [MaxLength(1000)]
    public string? Feedback { get; set; }

    public int? EnteredBy { get; set; }
    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Student Student { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }
    public virtual GradeComponent GradeComponent { get; set; }
    public virtual Teacher? EnteredByTeacher { get; set; }
}
```

---

## 9. MATERIALS ENTITIES

### 9.1 MaterialFolder

**POCO Class: MaterialFolder.cs**

```csharp
/// <summary>
/// Material folder/category entity
/// </summary>
public class MaterialFolder
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public int? ParentFolderId { get; set; } // For nested folders

    public int CourseOfferingId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual MaterialFolder? ParentFolder { get; set; }
    public virtual CourseOffering CourseOffering { get; set; }

    // Collections
    public virtual ICollection<MaterialFolder> SubFolders { get; set; } = new List<MaterialFolder>();
    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
```

### 9.2 Material

**POCO Class: Material.cs**

```csharp
/// <summary>
/// Course material entity
/// </summary>
public class Material
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public string FilePath { get; set; }

    [MaxLength(50)]
    public string? FileType { get; set; } // PDF, DOCX, PPTX, etc.

    public long FileSize { get; set; } // in bytes

    public int CourseOfferingId { get; set; }
    public int? FolderId { get; set; }

    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int? DownloadCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual CourseOffering CourseOffering { get; set; }
    public virtual MaterialFolder? Folder { get; set; }
    public virtual Teacher UploadedByTeacher { get; set; }
}
```

---

## 10. DATABASE CONTEXT (DbContext)

**ApplicationDbContext.cs**

```csharp
/// <summary>
/// Main database context using Identity Framework
/// </summary>
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,           // TUser
    ApplicationRole,          // TRole
    int,                      // TKey (using int instead of string)
    IdentityUserClaim<int>,   // TUserClaim
    ApplicationUserRole,      // TUserRole
    IdentityUserLogin<int>,   // TUserLogin
    ApplicationRoleClaim,     // TRoleClaim
    IdentityUserToken<int>>   // TUserToken
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Identity Tables (provided by Identity Framework)
    // AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, 
    // AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens

    // Custom DbSets
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<University> Universities { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<CourseOffering> CourseOfferings { get; set; }
    public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
    public DbSet<Timetable> Timetables { get; set; }
    public DbSet<ScheduledSlot> ScheduledSlots { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<ESPDevice> ESPDevices { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAssignment> QuizAssignments { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<GradeComponent> GradeComponents { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<MaterialFolder> MaterialFolders { get; set; }
    public DbSet<Material> Materials { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // CRITICAL: Call base first for Identity

        // Configure Identity tables with custom names (optional)
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<ApplicationUserRole>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<ApplicationRoleClaim>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        // Configure TPH Inheritance for ApplicationUser
        builder.Entity<ApplicationUser>()
            .HasDiscriminator<string>("UserType")
            .HasValue<ApplicationUser>("User")
            .HasValue<Student>("Student")
            .HasValue<Teacher>("Teacher");

        // Configure User-Role relationship
        builder.Entity<ApplicationUserRole>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId);

        builder.Entity<ApplicationUserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Student relationships
        builder.Entity<Student>()
            .HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne(s => s.Level)
            .WithMany(l => l.Students)
            .HasForeignKey(s => s.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne(s => s.Section)
            .WithMany(sec => sec.Students)
            .HasForeignKey(s => s.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Student>()
            .HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher relationships
        builder.Entity<Teacher>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Teachers)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Department Head relationship
        builder.Entity<Department>()
            .HasOne(d => d.HeadOfDepartment)
            .WithMany()
            .HasForeignKey(d => d.HeadOfDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course prerequisite (self-referencing)
        builder.Entity<Course>()
            .HasOne(c => c.PrerequisiteCourse)
            .WithMany(c => c.PrerequisiteFor)
            .HasForeignKey(c => c.PrerequisiteCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseOffering relationships
        builder.Entity<CourseOffering>()
            .HasOne(co => co.Course)
            .WithMany(c => c.Offerings)
            .HasForeignKey(co => co.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CourseOffering>()
            .HasOne(co => co.Teacher)
            .WithMany(t => t.CourseOfferings)
            .HasForeignKey(co => co.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Attendance relationships
        builder.Entity<Attendance>()
            .HasOne(a => a.MarkedByTeacher)
            .WithMany()
            .HasForeignKey(a => a.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Grade relationships
        builder.Entity<Grade>()
            .HasOne(g => g.EnteredByTeacher)
            .WithMany()
            .HasForeignKey(g => g.EnteredBy)
            .OnDelete(DeleteBehavior.Restrict);

        // MaterialFolder self-referencing
        builder.Entity<MaterialFolder>()
            .HasOne(mf => mf.ParentFolder)
            .WithMany(mf => mf.SubFolders)
            .HasForeignKey(mf => mf.ParentFolderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints
        builder.Entity<Course>()
            .HasIndex(c => c.Code)
            .IsUnique();

        builder.Entity<ESPDevice>()
            .HasIndex(d => d.DeviceId)
            .IsUnique();

        // Decimal precision
        builder.Entity<Grade>()
            .Property(g => g.Score)
            .HasColumnType("decimal(5,2)");

        builder.Entity<GradeComponent>()
            .Property(gc => gc.Weight)
            .HasColumnType("decimal(5,2)");

        builder.Entity<Student>()
            .Property(s => s.CurrentGPA)
            .HasColumnType("decimal(3,2)");

        // Indexes for performance
        builder.Entity<Session>()
            .HasIndex(s => s.SessionDate);

        builder.Entity<Attendance>()
            .HasIndex(a => a.CheckInTime);

        builder.Entity<QuizAttempt>()
            .HasIndex(qa => qa.StartedAt);
    }
}
```

---

## 11. MIGRATION COMMANDS

```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Add migration after changes
dotnet ef migrations add AddFaceEmbeddingBase64

# Generate SQL script
dotnet ef migrations script

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 12. CONNECTION STRING (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartClassRoom;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

---

## 13. SUMMARY

### Total Entities: 26

**Identity Framework (7 tables):**
- AspNetUsers → ApplicationUser (custom)
- AspNetRoles → ApplicationRole (custom)
- AspNetUserRoles → ApplicationUserRole (custom)
- AspNetUserClaims
- AspNetRoleClaims → ApplicationRoleClaim (custom)
- AspNetUserLogins
- AspNetUserTokens

**Custom Entities (19 classes):**
1. Student (extends ApplicationUser)
2. Teacher (extends ApplicationUser)
3. University
4. Department
5. Level
6. Section
7. Group
8. Course
9. Term
10. CourseOffering
11. StudentEnrollment
12. Timetable
13. ScheduledSlot
14. Session
15. Room
16. Attendance
17. ESPDevice
18. Quiz
19. QuizQuestion
20. QuizAssignment
21. QuizAttempt
22. GradeComponent
23. Grade
24. MaterialFolder
25. Material

### Key Features:
- ✅ Identity Framework integration
- ✅ Role-based access control
- ✅ TPH inheritance (ApplicationUser → Student/Teacher)
- ✅ Face embedding stored as Base64 string
- ✅ Self-referencing relationships (Course prerequisite, Material folders)
- ✅ Cascade delete restrictions
- ✅ Indexes for performance
- ✅ Decimal precision for grades/GPA

---

**END OF FILE 1**
