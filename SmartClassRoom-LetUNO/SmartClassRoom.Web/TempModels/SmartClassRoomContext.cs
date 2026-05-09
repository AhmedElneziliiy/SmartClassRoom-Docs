using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartClassRoom.Web.TempModels;

public partial class SmartClassRoomContext : DbContext
{
    public SmartClassRoomContext()
    {
    }

    public SmartClassRoomContext(DbContextOptions<SmartClassRoomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseOffering> CourseOfferings { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Espdevice> Espdevices { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeComponent> GradeComponents { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Level> Levels { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<MaterialFolder> MaterialFolders { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAssignment> QuizAssignments { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleClaim> RoleClaims { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<ScheduledSlot> ScheduledSlots { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<StudentEnrollment> StudentEnrollments { get; set; }

    public virtual DbSet<Term> Terms { get; set; }

    public virtual DbSet<Timetable> Timetables { get; set; }

    public virtual DbSet<University> Universities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserClaim> UserClaims { get; set; }

    public virtual DbSet<UserLogin> UserLogins { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasIndex(e => e.CheckInTime, "IX_Attendances_CheckInTime");

            entity.HasIndex(e => e.EspdeviceId, "IX_Attendances_ESPDeviceId");

            entity.HasIndex(e => e.MarkedBy, "IX_Attendances_MarkedBy");

            entity.HasIndex(e => e.SessionId, "IX_Attendances_SessionId");

            entity.HasIndex(e => e.StudentId, "IX_Attendances_StudentId");

            entity.Property(e => e.EspdeviceId).HasColumnName("ESPDeviceId");
            entity.Property(e => e.Latitude).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.VerificationMethod).HasMaxLength(50);

            entity.HasOne(d => d.Espdevice).WithMany(p => p.Attendances).HasForeignKey(d => d.EspdeviceId);

            entity.HasOne(d => d.MarkedByNavigation).WithMany(p => p.AttendanceMarkedByNavigations).HasForeignKey(d => d.MarkedBy);

            entity.HasOne(d => d.Session).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasIndex(e => e.Code, "IX_Courses_Code").IsUnique();

            entity.HasIndex(e => e.DepartmentId, "IX_Courses_DepartmentId");

            entity.HasIndex(e => e.PrerequisiteCourseId, "IX_Courses_PrerequisiteCourseId");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.CourseType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Department).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PrerequisiteCourse).WithMany(p => p.InversePrerequisiteCourse).HasForeignKey(d => d.PrerequisiteCourseId);
        });

        modelBuilder.Entity<CourseOffering>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_CourseOfferings_CourseId");

            entity.HasIndex(e => e.SectionId, "IX_CourseOfferings_SectionId");

            entity.HasIndex(e => e.TeacherId, "IX_CourseOfferings_TeacherId");

            entity.HasIndex(e => e.TermId, "IX_CourseOfferings_TermId");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseOfferings)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Section).WithMany(p => p.CourseOfferings).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.Teacher).WithMany(p => p.CourseOfferings)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Term).WithMany(p => p.CourseOfferings)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasIndex(e => e.HeadOfDepartmentId, "IX_Departments_HeadOfDepartmentId");

            entity.HasIndex(e => e.UniversityId, "IX_Departments_UniversityId");

            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.HeadOfDepartment).WithMany(p => p.Departments).HasForeignKey(d => d.HeadOfDepartmentId);

            entity.HasOne(d => d.University).WithMany(p => p.Departments)
                .HasForeignKey(d => d.UniversityId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Espdevice>(entity =>
        {
            entity.ToTable("ESPDevices");

            entity.HasIndex(e => e.DeviceId, "IX_ESPDevices_DeviceId").IsUnique();

            entity.HasIndex(e => e.RoomId, "IX_ESPDevices_RoomId");

            entity.Property(e => e.DeviceId).HasMaxLength(50);
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.MacAddress).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Room).WithMany(p => p.Espdevices).HasForeignKey(d => d.RoomId);
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_Grades_CourseOfferingId");

            entity.HasIndex(e => e.EnteredBy, "IX_Grades_EnteredBy");

            entity.HasIndex(e => e.GradeComponentId, "IX_Grades_GradeComponentId");

            entity.HasIndex(e => e.StudentId, "IX_Grades_StudentId");

            entity.Property(e => e.Feedback).HasMaxLength(1000);
            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.Grades)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.EnteredByNavigation).WithMany(p => p.GradeEnteredByNavigations).HasForeignKey(d => d.EnteredBy);

            entity.HasOne(d => d.GradeComponent).WithMany(p => p.Grades)
                .HasForeignKey(d => d.GradeComponentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.GradeStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<GradeComponent>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_GradeComponents_CourseOfferingId");

            entity.Property(e => e.ComponentName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MaxScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.GradeComponents)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasIndex(e => e.SectionId, "IX_Groups_SectionId");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Section).WithMany(p => p.Groups)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Level>(entity =>
        {
            entity.HasIndex(e => e.DepartmentId, "IX_Levels_DepartmentId");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.Levels)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_Materials_CourseOfferingId");

            entity.HasIndex(e => e.FolderId, "IX_Materials_FolderId");

            entity.HasIndex(e => e.UploadedByTeacherId, "IX_Materials_UploadedByTeacherId");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.Materials)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Folder).WithMany(p => p.Materials).HasForeignKey(d => d.FolderId);

            entity.HasOne(d => d.UploadedByTeacher).WithMany(p => p.Materials)
                .HasForeignKey(d => d.UploadedByTeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MaterialFolder>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_MaterialFolders_CourseOfferingId");

            entity.HasIndex(e => e.ParentFolderId, "IX_MaterialFolders_ParentFolderId");

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.MaterialFolders)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ParentFolder).WithMany(p => p.InverseParentFolder).HasForeignKey(d => d.ParentFolderId);
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasIndex(e => e.CourseId, "IX_Quizzes_CourseId");

            entity.HasIndex(e => e.CreatorId, "IX_Quizzes_CreatorId");

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PassingScore).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Creator).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuizAssignment>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_QuizAssignments_CourseOfferingId");

            entity.HasIndex(e => e.QuizId, "IX_QuizAssignments_QuizId");

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.QuizAssignments)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAssignments)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasIndex(e => e.QuizAssignmentId, "IX_QuizAttempts_QuizAssignmentId");

            entity.HasIndex(e => e.StartedAt, "IX_QuizAttempts_StartedAt");

            entity.HasIndex(e => e.StudentId, "IX_QuizAttempts_StudentId");

            entity.Property(e => e.Percentage).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Score).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TotalPoints).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.QuizAssignment).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizAssignmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasIndex(e => e.QuizId, "IX_QuizQuestions_QuizId");

            entity.Property(e => e.Explanation).HasMaxLength(1000);
            entity.Property(e => e.Points).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<RoleClaim>(entity =>
        {
            entity.HasIndex(e => e.ApplicationRoleId, "IX_RoleClaims_ApplicationRoleId");

            entity.HasIndex(e => e.RoleId, "IX_RoleClaims_RoleId");

            entity.HasOne(d => d.ApplicationRole).WithMany(p => p.RoleClaimApplicationRoles).HasForeignKey(d => d.ApplicationRoleId);

            entity.HasOne(d => d.Role).WithMany(p => p.RoleClaimRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(e => e.Building).HasMaxLength(100);
            entity.Property(e => e.Equipment).HasMaxLength(500);
            entity.Property(e => e.Floor).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Number).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<ScheduledSlot>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_ScheduledSlots_CourseOfferingId");

            entity.HasIndex(e => e.RoomId, "IX_ScheduledSlots_RoomId");

            entity.HasIndex(e => e.TimetableId, "IX_ScheduledSlots_TimetableId");

            entity.Property(e => e.DayOfWeek).HasMaxLength(20);

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.ScheduledSlots)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Room).WithMany(p => p.ScheduledSlots).HasForeignKey(d => d.RoomId);

            entity.HasOne(d => d.Timetable).WithMany(p => p.ScheduledSlots)
                .HasForeignKey(d => d.TimetableId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasIndex(e => e.LevelId, "IX_Sections_LevelId");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Level).WithMany(p => p.Sections)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_Sessions_CourseOfferingId");

            entity.HasIndex(e => e.RoomId, "IX_Sessions_RoomId");

            entity.HasIndex(e => e.SessionDate, "IX_Sessions_SessionDate");

            entity.HasIndex(e => e.TeacherId, "IX_Sessions_TeacherId");

            entity.HasIndex(e => e.TimetableId, "IX_Sessions_TimetableId");

            entity.Property(e => e.SessionType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Topic).HasMaxLength(500);

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Room).WithMany(p => p.Sessions).HasForeignKey(d => d.RoomId);

            entity.HasOne(d => d.Teacher).WithMany(p => p.Sessions).HasForeignKey(d => d.TeacherId);

            entity.HasOne(d => d.Timetable).WithMany(p => p.Sessions).HasForeignKey(d => d.TimetableId);
        });

        modelBuilder.Entity<StudentEnrollment>(entity =>
        {
            entity.HasIndex(e => e.CourseOfferingId, "IX_StudentEnrollments_CourseOfferingId");

            entity.HasIndex(e => e.StudentId, "IX_StudentEnrollments_StudentId");

            entity.Property(e => e.FinalGrade).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LetterGrade).HasMaxLength(5);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.CourseOffering).WithMany(p => p.StudentEnrollments)
                .HasForeignKey(d => d.CourseOfferingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.StudentEnrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Term>(entity =>
        {
            entity.Property(e => e.Code).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasIndex(e => e.CreatorId, "IX_Timetables_CreatorId");

            entity.HasIndex(e => e.DepartmentId, "IX_Timetables_DepartmentId");

            entity.HasIndex(e => e.LevelId, "IX_Timetables_LevelId");

            entity.HasIndex(e => e.SectionId, "IX_Timetables_SectionId");

            entity.HasIndex(e => e.TermId, "IX_Timetables_TermId");

            entity.HasIndex(e => e.TermId1, "IX_Timetables_TermId1");

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Creator).WithMany(p => p.Timetables).HasForeignKey(d => d.CreatorId);

            entity.HasOne(d => d.Department).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Level).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Section).WithMany(p => p.Timetables).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.Term).WithMany(p => p.TimetableTerms)
                .HasForeignKey(d => d.TermId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.TermId1Navigation).WithMany(p => p.TimetableTermId1Navigations).HasForeignKey(d => d.TermId1);
        });

        modelBuilder.Entity<University>(entity =>
        {
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.LogoPath).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.DepartmentId, "IX_Users_DepartmentId");

            entity.HasIndex(e => e.GroupId, "IX_Users_GroupId");

            entity.HasIndex(e => e.LevelId, "IX_Users_LevelId");

            entity.HasIndex(e => e.SectionId, "IX_Users_SectionId");

            entity.HasIndex(e => e.TeacherDepartmentId, "IX_Users_Teacher_DepartmentId");

            entity.HasIndex(e => e.UniversityId, "IX_Users_UniversityId");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.AcademicStatus).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CurrentGpa)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("CurrentGPA");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.EmployeeCode).HasMaxLength(20);
            entity.Property(e => e.EmploymentStatus).HasMaxLength(20);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.ProfilePhotoPath).HasMaxLength(255);
            entity.Property(e => e.Specialization).HasMaxLength(100);
            entity.Property(e => e.StudentCode).HasMaxLength(20);
            entity.Property(e => e.TeacherDepartmentId).HasColumnName("Teacher_DepartmentId");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Udid)
                .HasMaxLength(100)
                .HasColumnName("UDID");
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.UserType).HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.UserDepartments).HasForeignKey(d => d.DepartmentId);

            entity.HasOne(d => d.Group).WithMany(p => p.Users).HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Level).WithMany(p => p.Users).HasForeignKey(d => d.LevelId);

            entity.HasOne(d => d.Section).WithMany(p => p.Users).HasForeignKey(d => d.SectionId);

            entity.HasOne(d => d.TeacherDepartment).WithMany(p => p.UserTeacherDepartments).HasForeignKey(d => d.TeacherDepartmentId);

            entity.HasOne(d => d.University).WithMany(p => p.Users).HasForeignKey(d => d.UniversityId);
        });

        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_UserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.UserClaims)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_UserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.UserLogins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasIndex(e => e.RoleId, "IX_UserRoles_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
