using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Models.Entities.Academic;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Seed;

/// <summary>
/// Minimal seeder for Academic data (1 University, 2 Departments, minimal students/teachers)
/// </summary>
public static class AcademicDataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // Seed 1 University
        if (!await context.Universities.AnyAsync())
        {
            await SeedUniversityAsync(context);
        }

        // Seed 2 Departments
        if (!await context.Departments.AnyAsync())
        {
            await SeedDepartmentsAsync(context);
        }

        // Seed Levels (2 per department)
        if (!await context.Levels.AnyAsync())
        {
            await SeedLevelsAsync(context);
        }

        // Seed Sections (1 per level)
        if (!await context.Sections.AnyAsync())
        {
            await SeedSectionsAsync(context);
        }

        // Seed Groups (1 per section)
        if (!await context.Groups.AnyAsync())
        {
            await SeedGroupsAsync(context);
        }

        // Seed Teachers (2 per department)
        if (!await context.Teachers.AnyAsync())
        {
            await SeedTeachersAsync(context, userManager);
        }

        // Seed Students (2 per group)
        var studentCount = await context.Students.CountAsync();
        if (studentCount < 10)
        {
            await SeedStudentsAsync(context, userManager);
        }

        // Seed Courses (2 per department)
        if (!await context.Courses.AnyAsync())
        {
            await SeedCoursesAsync(context);
        }

        // Seed Rooms
        if (!await context.Rooms.AnyAsync())
        {
            await SeedRoomsAsync(context);
        }

        // Seed Terms
        if (!await context.Terms.AnyAsync())
        {
            await SeedTermsAsync(context);
        }

        // Seed Course Offerings
        if (!await context.CourseOfferings.AnyAsync())
        {
            await SeedCourseOfferingsAsync(context);
        }
    }

    private static async Task SeedUniversityAsync(ApplicationDbContext context)
    {
        var university = new University
        {
            Name = "Smart University",
            Code = "SU",
            Address = "Cairo, Egypt",
            Phone = "+20 2 3567 6900",
            Email = "info@smart.edu.eg",
            Website = "https://smart.edu.eg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddYears(-5)
        };

        await context.Universities.AddAsync(university);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDepartmentsAsync(ApplicationDbContext context)
    {
        var university = await context.Universities.FirstAsync();

        var departments = new List<Department>
        {
            new Department
            {
                Name = "Computer Science",
                Code = "CS",
                Description = "Computer Science Department",
                UniversityId = university.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddYears(-2)
            },
            new Department
            {
                Name = "Information Systems",
                Code = "IS",
                Description = "Information Systems Department",
                UniversityId = university.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddYears(-2)
            }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLevelsAsync(ApplicationDbContext context)
    {
        var departments = await context.Departments.ToListAsync();
        var levels = new List<Level>();

        foreach (var department in departments)
        {
            levels.AddRange(new[]
            {
                new Level
                {
                    Name = "Level 1",
                    LevelNumber = 1,
                    DepartmentId = department.Id,
                    IsActive = true
                },
                new Level
                {
                    Name = "Level 2",
                    LevelNumber = 2,
                    DepartmentId = department.Id,
                    IsActive = true
                }
            });
        }

        await context.Levels.AddRangeAsync(levels);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSectionsAsync(ApplicationDbContext context)
    {
        var levels = await context.Levels.ToListAsync();
        var sections = new List<Section>();

        foreach (var level in levels)
        {
            sections.Add(new Section
            {
                Name = "Section A",
                Code = "A",
                LevelId = level.Id,
                Capacity = 50,
                IsActive = true
            });
        }

        await context.Sections.AddRangeAsync(sections);
        await context.SaveChangesAsync();
    }

    private static async Task SeedGroupsAsync(ApplicationDbContext context)
    {
        var sections = await context.Sections.ToListAsync();
        var groups = new List<Group>();

        foreach (var section in sections)
        {
            groups.Add(new Group
            {
                Name = "Group 1",
                Code = "G1",
                SectionId = section.Id,
                Capacity = 20,
                IsActive = true
            });
        }

        await context.Groups.AddRangeAsync(groups);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTeachersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        var departments = await context.Departments.ToListAsync();
        int teacherCount = 1;

        foreach (var department in departments)
        {
            // Create 2 teachers per department
            for (int i = 1; i <= 2; i++)
            {
                var teacher = new Teacher
                {
                    FullName = $"Dr. Teacher {teacherCount}",
                    UserName = $"teacher{teacherCount}@smart.edu",
                    Email = $"teacher{teacherCount}@smart.edu",
                    PhoneNumber = $"+20 100 {new Random().Next(1000000, 9999999)}",
                    NationalId = $"29{new Random().Next(10000000, 99999999):D8}{new Random().Next(10000, 99999):D5}",
                    DateOfBirth = DateTime.UtcNow.AddYears(-35),
                    Gender = "Male",
                    UserType = "Teacher",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),

                    // Teacher-specific properties
                    EmployeeCode = $"T{teacherCount:D5}",
                    UniversityId = department.UniversityId,
                    DepartmentId = department.Id,
                    Title = "Professor",
                    Specialization = "Computer Science",
                    HireDate = DateTime.UtcNow.AddYears(-5),
                    EmploymentStatus = "Full-Time"
                };

                var result = await userManager.CreateAsync(teacher, "Pass@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher, "Teacher");
                }

                teacherCount++;
            }

            // Assign first teacher as Head of Department
            var firstTeacher = await context.Teachers
                .Where(t => t.DepartmentId == department.Id)
                .OrderBy(t => t.Id)
                .FirstOrDefaultAsync();

            if (firstTeacher != null)
            {
                department.HeadOfDepartmentId = firstTeacher.Id;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedStudentsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        // Load structure: Departments -> Levels -> Sections -> Groups
        var departments = await context.Departments.ToListAsync();
        var levels = await context.Levels.ToListAsync();
        var sections = await context.Sections.ToListAsync();
        var groups = await context.Groups.ToListAsync();

        var existingStudentCount = await context.Students.CountAsync();
        int studentCount = Math.Max(existingStudentCount + 1, 100);

        var random = new Random(42);

        // Create 2 students per group
        foreach (var group in groups)
        {
            var section = sections.First(s => s.Id == group.SectionId);
            var level = levels.First(l => l.Id == section.LevelId);
            var department = departments.First(d => d.Id == level.DepartmentId);

            for (int i = 0; i < 2; i++)
            {
                var student = new Student
                {
                    FullName = $"Student {studentCount}",
                    UserName = $"student{studentCount}@smart.edu",
                    Email = $"student{studentCount}@smart.edu",
                    PhoneNumber = $"+20 101 {random.Next(1000000, 9999999)}",
                    NationalId = $"3{random.Next(10000000, 99999999):D8}{random.Next(10000, 99999):D5}",
                    DateOfBirth = DateTime.UtcNow.AddYears(-20),
                    Gender = i % 2 == 0 ? "Male" : "Female",
                    UserType = "Student",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-12),

                    // Student-specific properties
                    StudentCode = $"S{studentCount:D6}",
                    UniversityId = department.UniversityId,
                    DepartmentId = department.Id,
                    LevelId = level.Id,
                    SectionId = section.Id,
                    GroupId = group.Id,
                    EnrollmentDate = DateTime.UtcNow.AddYears(-(level.LevelNumber - 1)),
                    AcademicStatus = "Active",
                    CurrentGPA = 3.5m
                };

                var result = await userManager.CreateAsync(student, "Pass@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                }

                studentCount++;
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCoursesAsync(ApplicationDbContext context)
    {
        var departments = await context.Departments.ToListAsync();
        var courses = new List<Course>();

        foreach (var department in departments)
        {
            courses.AddRange(new[]
            {
                new Course
                {
                    Name = "Introduction to Programming",
                    Code = $"{department.Code}101",
                    Description = "Basic programming concepts",
                    Credits = 3,
                    CourseType = "Core",
                    DepartmentId = department.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddYears(-1)
                },
                new Course
                {
                    Name = "Data Structures",
                    Code = $"{department.Code}102",
                    Description = "Data structures and algorithms",
                    Credits = 4,
                    CourseType = "Core",
                    DepartmentId = department.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddYears(-1)
                }
            });
        }

        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoomsAsync(ApplicationDbContext context)
    {
        var rooms = new List<Models.Entities.Scheduling.Room>
        {
            // Lecture Halls
            new() { Number = "LH-101", Name = "Lecture Hall 101", RoomType = "Lecture", Capacity = 50, Building = "Building A", Floor = "1", Equipment = "Projector, Whiteboard, Microphone", Status = "Available", IsActive = true },
            new() { Number = "LH-102", Name = "Lecture Hall 102", RoomType = "Lecture", Capacity = 60, Building = "Building A", Floor = "1", Equipment = "Projector, Whiteboard, Sound System", Status = "Available", IsActive = true },
            new() { Number = "LH-201", Name = "Lecture Hall 201", RoomType = "Lecture", Capacity = 80, Building = "Building A", Floor = "2", Equipment = "Smart Board, Projector, Microphone", Status = "Available", IsActive = true },
            new() { Number = "LH-202", Name = "Lecture Hall 202", RoomType = "Lecture", Capacity = 100, Building = "Building A", Floor = "2", Equipment = "Smart Board, Dual Projectors, Sound System", Status = "Available", IsActive = true },
            new() { Number = "AUD-301", Name = "Main Auditorium", RoomType = "Auditorium", Capacity = 200, Building = "Building A", Floor = "3", Equipment = "Stage, Sound System, Projector, Lighting", Status = "Available", IsActive = true },

            // Computer Labs
            new() { Number = "LAB-101", Name = "Computer Lab 1", RoomType = "Lab", Capacity = 30, Building = "Building B", Floor = "1", Equipment = "30 PCs, Projector, Whiteboard", Status = "Available", IsActive = true },
            new() { Number = "LAB-102", Name = "Computer Lab 2", RoomType = "Lab", Capacity = 30, Building = "Building B", Floor = "1", Equipment = "30 PCs, Smart Board, Network Equipment", Status = "Available", IsActive = true },
            new() { Number = "LAB-201", Name = "Computer Lab 3", RoomType = "Lab", Capacity = 25, Building = "Building B", Floor = "2", Equipment = "25 PCs, Projector, Whiteboard", Status = "Available", IsActive = true },
            new() { Number = "LAB-202", Name = "Programming Lab", RoomType = "Lab", Capacity = 35, Building = "Building B", Floor = "2", Equipment = "35 PCs, Dual Monitors, Projector", Status = "Available", IsActive = true },
            new() { Number = "LAB-301", Name = "Advanced Lab", RoomType = "Lab", Capacity = 20, Building = "Building B", Floor = "3", Equipment = "20 High-End PCs, Server Rack, Projector", Status = "Available", IsActive = true }
        };

        await context.Rooms.AddRangeAsync(rooms);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTermsAsync(ApplicationDbContext context)
    {
        var terms = new List<Term>
        {
            new Term
            {
                Name = "Fall 2024",
                Code = "FALL2024",
                StartDate = new DateTime(2024, 9, 1),
                EndDate = new DateTime(2024, 12, 31),
                Status = "Active",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new Term
            {
                Name = "Spring 2025",
                Code = "SPRING2025",
                StartDate = new DateTime(2025, 2, 1),
                EndDate = new DateTime(2025, 5, 31),
                Status = "Upcoming",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Terms.AddRangeAsync(terms);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCourseOfferingsAsync(ApplicationDbContext context)
    {
        var courses = await context.Courses.ToListAsync();
        var terms = await context.Terms.ToListAsync();
        var teachers = await context.Teachers.ToListAsync();
        var sections = await context.Sections.ToListAsync();

        if (!courses.Any() || !terms.Any() || !teachers.Any() || !sections.Any())
            return;

        var activeTerm = terms.FirstOrDefault(t => t.Status == "Active");
        if (activeTerm == null)
            return;

        var offerings = new List<Models.Entities.Academic.CourseOffering>();

        // Create 2-3 offerings per section
        foreach (var section in sections.Take(2)) // First 2 sections
        {
            var sectionCourses = courses.Take(4).ToList(); // 4 courses per section
            var teacherIndex = 0;

            foreach (var course in sectionCourses)
            {
                var teacher = teachers[teacherIndex % teachers.Count];

                offerings.Add(new Models.Entities.Academic.CourseOffering
                {
                    CourseId = course.Id,
                    TermId = activeTerm.Id,
                    TeacherId = teacher.Id,
                    SectionId = section.Id,
                    SessionsPerWeek = (teacherIndex % 3) + 2, // 2, 3, or 4 sessions per week
                    MaxStudents = 30,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow.AddMonths(-2)
                });

                teacherIndex++;
            }
        }

        await context.CourseOfferings.AddRangeAsync(offerings);
        await context.SaveChangesAsync();
    }
}
