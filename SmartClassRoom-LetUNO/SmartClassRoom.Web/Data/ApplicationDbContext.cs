using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Scheduling;
using SmartClassRoom.Web.Models.Entities.Attendance;
using SmartClassRoom.Web.Models.Entities.Quizzes;
using SmartClassRoom.Web.Models.Entities.Grading;
using SmartClassRoom.Web.Models.Entities.Materials;

namespace SmartClassRoom.Web.Data;

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
    public DbSet<Models.Entities.Attendance.Attendance> Attendances { get; set; }
    public DbSet<ESPDevice> ESPDevices { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizAssignment> QuizAssignments { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<GradeComponent> GradeComponents { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<GradeComponentTemplate> GradeComponentTemplates { get; set; }
    public DbSet<GradeComponentTemplateItem> GradeComponentTemplateItems { get; set; }
    public DbSet<MaterialFolder> MaterialFolders { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<TeacherAttendance> TeacherAttendances { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // CRITICAL: Call base first for Identity

        // Global configuration: Set all foreign keys to Restrict to avoid cascade cycles
        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

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
            .HasValue<ApplicationUser>("Admin")  // Admin users use base ApplicationUser type
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
        builder.Entity<Models.Entities.Attendance.Attendance>()
            .HasOne(a => a.MarkedByTeacher)
            .WithMany()
            .HasForeignKey(a => a.MarkedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Grade relationships
        builder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Grade>()
            .HasOne(g => g.CourseOffering)
            .WithMany(co => co.Grades)
            .HasForeignKey(g => g.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Grade>()
            .HasOne(g => g.GradeComponent)
            .WithMany(gc => gc.Grades)
            .HasForeignKey(g => g.GradeComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Grade>()
            .HasOne(g => g.EnteredByTeacher)
            .WithMany()
            .HasForeignKey(g => g.EnteredBy)
            .OnDelete(DeleteBehavior.Restrict);

        // GradeComponentTemplate relationships
        builder.Entity<GradeComponentTemplate>()
            .HasOne(t => t.Department)
            .WithMany()
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GradeComponentTemplate>()
            .HasMany(t => t.Items)
            .WithOne(i => i.Template)
            .HasForeignKey(i => i.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Timetable relationships
        builder.Entity<Timetable>()
            .HasOne(t => t.Department)
            .WithMany()
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Timetable>()
            .HasOne(t => t.Level)
            .WithMany()
            .HasForeignKey(t => t.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Timetable>()
            .HasOne(t => t.Section)
            .WithMany()
            .HasForeignKey(t => t.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Timetable>()
            .HasOne(t => t.Term)
            .WithMany()
            .HasForeignKey(t => t.TermId)
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

        builder.Entity<Models.Entities.Attendance.Attendance>()
            .HasIndex(a => a.CheckInTime);

        builder.Entity<QuizAttempt>()
            .HasIndex(qa => qa.StartedAt);

        // Configure cascade delete for Timetable -> Session -> Attendance hierarchy
        // When a Timetable is deleted, its Sessions should be deleted (Cascade)
        builder.Entity<Session>()
            .HasOne(s => s.Timetable)
            .WithMany()
            .HasForeignKey(s => s.TimetableId)
            .OnDelete(DeleteBehavior.Cascade);

        // When a Session is deleted, its Attendance records should be deleted (Cascade)
        builder.Entity<Models.Entities.Attendance.Attendance>()
            .HasOne(a => a.Session)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // TeacherAttendance relationships and indexes
        // When a Session is deleted, its TeacherAttendance records should be deleted (Cascade)
        builder.Entity<TeacherAttendance>()
            .HasOne(ta => ta.Session)
            .WithMany()
            .HasForeignKey(ta => ta.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeacherAttendance>()
            .HasOne(ta => ta.Teacher)
            .WithMany()
            .HasForeignKey(ta => ta.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeacherAttendance>()
            .HasIndex(ta => ta.CheckInTime);

        builder.Entity<TeacherAttendance>()
            .HasIndex(ta => new { ta.TeacherId, ta.SessionId })
            .IsUnique();
    }
}
