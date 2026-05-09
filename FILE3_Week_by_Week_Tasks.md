# FILE 3: WEEK-BY-WEEK DEVELOPMENT TASKS
## SmartClassRoom-LetUNO - 30-Day Implementation Plan

**Version**: 1.0  
**Date**: December 4, 2025  
**Duration**: 30 Days (4 Weeks)  
**Approach**: Sequential Development (Complete each week before next)

---

## DEVELOPMENT PRINCIPLES

### ⚠️ CRITICAL RULES

1. **Sequential Execution**: Complete Week N before starting Week N+1
2. **No Parallel Development**: Tasks within each week MUST be done in order
3. **Testing Per Week**: Test all features before moving to next week
4. **No Skipping**: Every task must be completed
5. **Documentation**: Update docs after each major feature

### Why Sequential?

- ✅ **Dependencies**: Week 2 needs Week 1's foundation
- ✅ **Integration**: Ensure each layer works before building next
- ✅ **Bug Prevention**: Fix issues early before they compound
- ✅ **Learning**: Understand each component fully
- ✅ **Quality**: Maintain high standards throughout

---

## WEEK 1: FOUNDATION & CORE SETUP
### Days 1-7 | Foundation Layer

**Goal**: Establish database, authentication, and basic user management

**Prerequisites**: None (starting from scratch)

**Deliverables**: Working authentication system with user management

---

### DAY 1-2: PROJECT SETUP & DATABASE

**Tasks** (Must complete in order):

#### 1.1 Create Solution & Project
```bash
# Step 1: Create solution
dotnet new sln -n SmartClassRoom

# Step 2: Create web project
dotnet new mvc -n SmartClassRoom.Web

# Step 3: Add project to solution
dotnet sln add SmartClassRoom.Web/SmartClassRoom.Web.csproj
```

#### 1.2 Install NuGet Packages
```bash
cd SmartClassRoom.Web

# Identity & EF Core
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Utilities
dotnet add package EPPlus
dotnet add package Swashbuckle.AspNetCore
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package Serilog.AspNetCore
```

#### 1.3 Create Folder Structure
Create ALL folders from FILE 2:
- Models/Entities/
- Models/ViewModels/
- Models/DTOs/
- Data/Repositories/
- Services/
- Controllers/Api/
- Areas/Admin/
- Utilities/

#### 1.4 Create POCO Classes
Copy ALL entity classes from FILE 1:
- ApplicationUser, ApplicationRole (Identity)
- Student, Teacher
- University, Department, Level, Section, Group
- Course, Term, CourseOffering
- All other entities

#### 1.5 Create ApplicationDbContext
- Implement DbContext (from FILE 1)
- Configure Identity tables
- Configure entity relationships
- Add indexes and constraints

#### 1.6 Configure appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartClassRoom;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "[GENERATE 32+ CHAR KEY]",
    "Issuer": "SmartClassRoomAPI",
    "Audience": "SmartClassRoomClients",
    "ExpiryMinutes": 60
  }
}
```

#### 1.7 Configure Program.cs
- Add DbContext with SQL Server
- Configure Identity Framework
- Configure JWT authentication
- Add authorization policies
- Register repositories (placeholder)
- Register services (placeholder)

#### 1.8 Create Initial Migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 1.9 Verify Database
- Open SQL Server Management Studio
- Confirm database created
- Check all tables exist (Identity + custom)
- Verify relationships/foreign keys

**Checkpoint**: Database must be fully created with all tables before proceeding.

---

### DAY 3-4: AUTHENTICATION & IDENTITY

**Tasks** (Must complete in order):

#### 2.1 Create IdentitySeeder
- Seed roles: Admin, Teacher, Student
- Create default admin user
- Test in Program.cs startup

#### 2.2 Implement IAuthService Interface
```csharp
public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<LoginResult> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email);
}
```

#### 2.3 Implement AuthService
- Use UserManager<ApplicationUser>
- Use SignInManager<ApplicationUser>
- Generate JWT tokens
- Validate credentials
- Handle lockout

#### 2.4 Create Login DTOs
- LoginRequestDto (email, password)
- LoginResponseDto (token, user info, roles)
- ChangePasswordDto

#### 2.5 Create AuthController (API)
```csharp
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
}
```

#### 2.6 Test Authentication
- Use Swagger UI
- POST /api/auth/login with admin credentials
- Verify JWT token returned
- Test change password endpoint

**Checkpoint**: Must be able to login and receive JWT token before proceeding.

---

### DAY 5-7: USER MANAGEMENT

**Tasks** (Must complete in order):

#### 3.1 Create Generic Repository
- IRepository<T> interface
- Repository<T> implementation
- CRUD operations
- Pagination support

#### 3.2 Create User Repositories
- IUserRepository interface
- UserRepository implementation
- IStudentRepository
- StudentRepository
- ITeacherRepository
- TeacherRepository

#### 3.3 Create UserService
```csharp
public interface IUserService
{
    Task<UserResponseDto> CreateUserAsync(CreateUserViewModel model);
    Task<bool> UpdateUserAsync(int id, EditUserViewModel model);
    Task<bool> DeleteUserAsync(int id);
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<(int Success, int Failed)> BulkImportAsync(IFormFile file);
}
```

#### 3.4 Implement UserService
- Use UserManager for user operations
- Handle role assignments
- Validate user data
- Store face embeddings (Base64)

#### 3.5 Create User ViewModels
- UserListViewModel
- CreateUserViewModel (with role selection)
- EditUserViewModel
- BulkImportViewModel

#### 3.6 Create UsersController (Admin MVC)
```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    public async Task<IActionResult> Index()
    public async Task<IActionResult> Create()
    [HttpPost] public async Task<IActionResult> Create(CreateUserViewModel model)
    public async Task<IActionResult> Edit(int id)
    [HttpPost] public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    public async Task<IActionResult> BulkImport()
    [HttpPost] public async Task<IActionResult> BulkImport(IFormFile file)
}
```

#### 3.7 Create User Views
- Index.cshtml (list with search/filter)
- Create.cshtml (form with role dropdown)
- Edit.cshtml
- BulkImport.cshtml (file upload)

#### 3.8 Implement Excel Import
- Use EPPlus library
- Parse Excel file
- Validate data
- Create users in bulk
- Return success/error report

#### 3.9 Implement Face Photo Upload
- Accept image file (IFormFile)
- Save to wwwroot/uploads/faces/{userId}.jpg
- Call your face recognition package
- Get Base64 embedding
- Store in ApplicationUser.FaceEmbeddingBase64

#### 3.10 Test User Management
- Create users manually
- Bulk import from Excel
- Upload face photos
- Edit/delete users
- Test with all 3 roles

**Week 1 Checkpoint**: 
- ✅ Database created and seeded
- ✅ Login working (API)
- ✅ User CRUD working (MVC)
- ✅ Bulk import working
- ✅ Face photos can be uploaded

**DO NOT PROCEED TO WEEK 2 UNTIL ALL ABOVE COMPLETED**

---

## WEEK 2: SCHEDULING & ATTENDANCE
### Days 8-14 | Timetable & Sessions

**Goal**: Implement automatic timetable generation and attendance system

**Prerequisites**: Week 1 must be 100% complete

**Deliverables**: Generated timetables with face recognition check-in

---

### DAY 8-9: ACADEMIC STRUCTURE

**Tasks**:

#### 4.1 Create Department/Level/Section Repositories
- IDepartmentRepository + implementation
- ILevelRepository + implementation
- ISectionRepository + implementation
- IGroupRepository + implementation

#### 4.2 Create Academic Services
- DepartmentService (CRUD)
- LevelService
- SectionService
- GroupService

#### 4.3 Create MVC Controllers
- DepartmentsController (Admin area)
- LevelsController
- SectionsController

#### 4.4 Create Views
- Department management pages
- Level management pages
- Section management pages

#### 4.5 Test Academic Structure
- Create departments
- Create levels for department
- Create sections for level
- Create groups for section

**Checkpoint**: Academic hierarchy must be working before timetable.

---

### DAY 10-11: COURSE MANAGEMENT

**Tasks**:

#### 5.1 Create Course Repositories
- ICourseRepository + implementation
- ITermRepository + implementation
- IOfferingRepository + implementation

#### 5.2 Create Course Services
- CourseService (CRUD + prerequisite handling)
- TermService
- OfferingService (with enrollment)

#### 5.3 Create MVC Controllers
- CoursesController
- TermsController
- OfferingsController

#### 5.4 Create Views
- Course catalog management
- Term management
- Course offering creation
- Student enrollment interface

#### 5.5 Test Course System
- Create courses with prerequisites
- Create academic terms
- Create course offerings
- Enroll students

**Checkpoint**: Must be able to create offerings with enrolled students.

---

### DAY 12-13: TIMETABLE GENERATION

**Tasks**:

#### 6.1 Create Timetable Entities
Ensure these exist:
- Timetable
- ScheduledSlot
- Room

#### 6.2 Create Repositories
- ITimetableRepository
- IScheduledSlotRepository
- IRoomRepository

#### 6.3 Implement TimetableService
**CRITICAL ALGORITHM**:

```csharp
public interface ITimetableService
{
    Task<TimetableGenerationResult> GenerateTimetableAsync(GenerateTimetableRequest request);
    Task<List<Conflict>> DetectConflictsAsync(int timetableId);
    Task<bool> PublishTimetableAsync(int timetableId);
}

// Algorithm:
// 1. Get all course offerings for dept/level/section/term
// 2. Get available rooms
// 3. Define time slots (8:00-9:00, 9:00-10:00, etc.)
// 4. For each offering (sorted by sessions/week DESC):
//    a. Find available time slot
//    b. Check teacher availability (no conflicts)
//    c. Check section availability (no conflicts)
//    d. Find suitable room (capacity + type)
//    e. Create ScheduledSlot
// 5. Return result with conflicts if any
```

#### 6.4 Create TimetableController (MVC)
```csharp
public async Task<IActionResult> Generate()  // Form to start generation
[HttpPost] public async Task<IActionResult> Generate(GenerateViewModel model)
public async Task<IActionResult> Review(int id)  // Review generated timetable
[HttpPost] public async Task<IActionResult> Publish(int id)  // Create sessions
```

#### 6.5 Create Timetable Views
- Generate.cshtml (select dept/level/section/term)
- Review.cshtml (grid view with conflicts highlighted)
- Index.cshtml (list all timetables)

#### 6.6 Test Timetable Generation
- Generate for a section
- Verify no teacher conflicts
- Verify no room conflicts
- Check conflict detection works
- Test manual adjustments
- Publish and verify sessions created

**Checkpoint**: Timetable generation MUST work before attendance.

---

### DAY 14: SESSION & ATTENDANCE SETUP

**Tasks**:

#### 7.1 Create Session/Attendance Repositories
- ISessionRepository
- IAttendanceRepository
- IESPDeviceRepository

#### 7.2 Create Session Service
```csharp
public interface ISessionService
{
    Task<List<SessionDto>> GetTodaysSessionsAsync(int userId, string role);
    Task<SessionDetailDto> GetSessionDetailAsync(int id);
    Task<Session> StartSessionAsync(int id);
    Task<Session> EndSessionAsync(int id);
}
```

#### 7.3 Create Sessions API Controller
```csharp
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    [HttpGet("today")]  // For mobile app
    [HttpGet("{id}")]  // Session details
}
```

#### 7.4 Test Session APIs
- GET /api/sessions/today (as student)
- GET /api/sessions/today (as teacher)
- GET /api/sessions/{id}

**Week 2 Checkpoint**:
- ✅ Academic structure complete
- ✅ Course management working
- ✅ Timetable generation successful
- ✅ Sessions created from timetable
- ✅ Session APIs working

**DO NOT PROCEED TO WEEK 3 UNTIL ALL ABOVE COMPLETED**

---

## WEEK 3: ASSESSMENT & GRADING
### Days 15-21 | Quizzes & GPA

**Goal**: Implement quiz system with auto-grading and comprehensive grading

**Prerequisites**: Week 2 must be 100% complete

**Deliverables**: Working quiz system + GPA calculation

---

### DAY 15-16: QUIZ SYSTEM

**Tasks**:

#### 8.1 Create Quiz Repositories
- IQuizRepository
- IQuizQuestionRepository
- IQuizAssignmentRepository
- IQuizAttemptRepository

#### 8.2 Implement QuizService
```csharp
public interface IQuizService
{
    // Teacher
    Task<int> CreateQuizAsync(CreateQuizViewModel model);
    Task<bool> AssignQuizAsync(int quizId, AssignQuizViewModel model);

    // Student
    Task<QuizDto> StartQuizAsync(int assignmentId, int studentId);
    Task<QuizResultDto> SubmitQuizAsync(int attemptId, Dictionary<int, int> answers);
    Task<QuizResultDto> GradeQuizAsync(int attemptId);  // Auto-grading
}
```

#### 8.3 Implement Auto-Grading Logic
**CRITICAL ALGORITHM**:

```csharp
public async Task<QuizResultDto> GradeQuizAsync(int attemptId)
{
    // 1. Get attempt with questions
    var attempt = await GetAttemptAsync(attemptId);

    // 2. Parse student answers from JSON
    var answers = JsonSerializer.Deserialize<Dictionary<int, int>>(attempt.Answers);

    // 3. Compare with correct answers
    decimal score = 0;
    foreach (var question in attempt.Quiz.Questions)
    {
        if (answers.TryGetValue(question.Id, out int studentAnswer))
        {
            if (studentAnswer == question.CorrectAnswer)
            {
                score += question.Points;
            }
        }
    }

    // 4. Calculate percentage
    var totalPoints = attempt.Quiz.Questions.Sum(q => q.Points);
    var percentage = (score / totalPoints) * 100;

    // 5. Determine pass/fail
    var status = percentage >= attempt.Quiz.PassingScore ? "Passed" : "Failed";

    // 6. Save results
    attempt.Score = score;
    attempt.TotalPoints = totalPoints;
    attempt.Percentage = percentage;
    attempt.Status = status;

    await SaveAsync(attempt);

    return CreateResult(attempt);
}
```

#### 8.4 Create Quiz MVC Controllers
```csharp
[Area("Admin")]
public class QuizzesController : Controller
{
    public async Task<IActionResult> Create()
    [HttpPost] public async Task<IActionResult> Create(CreateQuizViewModel model)
    public async Task<IActionResult> Assign(int id)
    [HttpPost] public async Task<IActionResult> Assign(AssignQuizViewModel model)
    public async Task<IActionResult> Submissions(int assignmentId)
}
```

#### 8.5 Create Quiz API Controllers
```csharp
[Route("api/[controller]")]
public class QuizzesController : ControllerBase
{
    [HttpGet("available")]  // Student's available quizzes
    [HttpPost("start/{assignmentId}")]  // Start quiz attempt
    [HttpPost("submit")]  // Submit answers
    [HttpGet("results/{attemptId}")]  // View results
}
```

#### 8.6 Test Quiz System
- Teacher creates quiz with 10 questions
- Teacher assigns to offering
- Student starts quiz (API)
- Student submits answers (API)
- Verify auto-grading works
- Check score calculation
- Verify pass/fail logic

**Checkpoint**: Quiz auto-grading MUST work correctly.

---

### DAY 17-18: FACE RECOGNITION ATTENDANCE

**Tasks**:

#### 9.1 Wrap Your Face Recognition Package
```csharp
public interface IFaceRecognitionService
{
    Task<string> GetEmbeddingBase64Async(byte[] imageBytes);
    Task<bool> VerifyFaceAsync(byte[] imageBytes, string storedEmbeddingBase64);
    Task<FaceMatchResult> RecognizeFaceAsync(byte[] imageBytes, List<string> embeddings);
}

public class FaceRecognitionService : IFaceRecognitionService
{
    // Call your custom face recognition package here
    public async Task<string> GetEmbeddingBase64Async(byte[] imageBytes)
    {
        // Use your package to get embedding
        // Convert to Base64 string
        // Return Base64
    }

    public async Task<bool> VerifyFaceAsync(byte[] imageBytes, string storedEmbeddingBase64)
    {
        // Get embedding from image
        // Compare with stored embedding
        // Return true if match (threshold: 0.6)
    }
}
```

#### 9.2 Implement AttendanceService
```csharp
public interface IAttendanceService
{
    Task<CheckInResult> CheckInAsync(int sessionId, int studentId, byte[] faceImage, decimal lat, decimal lng);
    Task<List<AttendanceDto>> GetSessionAttendanceAsync(int sessionId);
    Task<AttendanceSummaryDto> GetStudentAttendanceAsync(int studentId, int offeringId);
}
```

#### 9.3 Create Attendance API Controller
```csharp
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    [HttpPost("check-in")]
    public async Task<ActionResult<CheckInResult>> CheckIn([FromForm] CheckInRequest request)
    {
        // 1. Validate session is active
        // 2. Get student face embedding from database
        // 3. Verify face from uploaded image
        // 4. Check location (GPS)
        // 5. Record attendance
    }

    [HttpGet("my-attendance/{offeringId}")]
    public async Task<ActionResult<AttendanceSummaryDto>> GetMyAttendance(int offeringId)
}
```

#### 9.4 Test Face Recognition
- Student uploads face photo during registration
- Embedding stored as Base64
- Student checks in to session (API)
- Upload different photo of same student
- Verify face recognition succeeds
- Upload different person's photo
- Verify face recognition fails

**Checkpoint**: Face recognition check-in MUST work.

---

### DAY 19-21: GRADING SYSTEM & GPA

**Tasks**:

#### 10.1 Create Grading Repositories
- IGradeComponentRepository
- IGradeRepository

#### 10.2 Implement GradingService
**CRITICAL ALGORITHMS**:

```csharp
public interface IGradingService
{
    // Grade components
    Task<bool> SaveGradeComponentsAsync(int offeringId, List<ComponentDto> components);

    // Grade entry
    Task<bool> SaveGradeAsync(SaveGradeDto dto);

    // Final grade calculation
    Task<CalculationResult> CalculateFinalGradesAsync(int offeringId);

    // GPA calculation
    Task<GPADto> CalculateGPAAsync(int studentId, int? termId);

    // Transcript
    Task<byte[]> GenerateTranscriptPdfAsync(int studentId);
}

// Final Grade Algorithm:
// FinalScore = Σ(ComponentScore × ComponentWeight)
// Example: Midterm(85 × 0.30) + Final(90 × 0.40) + Assignment(88 × 0.20) + Attendance(95 × 0.10)

// Letter Grade Conversion:
// A:  90-100%  → 4.0
// B+: 85-89%   → 3.3
// B:  80-84%   → 3.0
// C+: 75-79%   → 2.3
// C:  70-74%   → 2.0
// D:  60-69%   → 1.0
// F:  <60%     → 0.0

// GPA Calculation:
// GPA = Σ(GradePoint × Credits) / Σ(Credits)
// Example: Course1(A/4.0 × 3) + Course2(B+/3.3 × 3) + Course3(A-/3.7 × 4) = 36.7 / 10 = 3.67
```

#### 10.3 Create Grades MVC Controller
```csharp
[Area("Admin")]
public class GradesController : Controller
{
    public async Task<IActionResult> ConfigureComponents(int offeringId)
    [HttpPost] public async Task<IActionResult> SaveComponents(ComponentsViewModel model)

    public async Task<IActionResult> GradeSheet(int offeringId)
    [HttpPost] public async Task<IActionResult> SaveGrade(SaveGradeDto dto)

    [HttpPost] public async Task<IActionResult> CalculateFinalGrades(int offeringId)
}
```

#### 10.4 Create Grades API Controller
```csharp
[Route("api/[controller]")]
public class GradesController : ControllerBase
{
    [HttpGet("my-courses")]  // Student's courses with grades
    [HttpGet("course/{offeringId}")]  // Detailed grade breakdown
    [HttpGet("gpa")]  // Student's GPA
    [HttpGet("transcript")]  // Download transcript PDF
}
```

#### 10.5 Test Grading System
- Configure grade components (Midterm 30%, Final 40%, etc.)
- Enter grades for students
- Calculate final grades
- Verify letter grade conversion
- Calculate GPA for a student
- Generate transcript PDF
- Test via mobile API

**Week 3 Checkpoint**:
- ✅ Quiz system working with auto-grading
- ✅ Face recognition check-in functional
- ✅ Grading system complete
- ✅ GPA calculation accurate
- ✅ Transcript generation working

**DO NOT PROCEED TO WEEK 4 UNTIL ALL ABOVE COMPLETED**

---

## WEEK 4: MATERIALS, REPORTS & DEPLOYMENT
### Days 22-30 | Polish & Production

**Goal**: Complete remaining features, test thoroughly, deploy

**Prerequisites**: Week 3 must be 100% complete

**Deliverables**: Production-ready system deployed

---

### DAY 22-23: MATERIALS MANAGEMENT

**Tasks**:

#### 11.1 Create Material Repositories
- IMaterialFolderRepository
- IMaterialRepository

#### 11.2 Implement MaterialService
```csharp
public interface IMaterialService
{
    Task<int> CreateFolderAsync(CreateFolderDto dto);
    Task<int> UploadMaterialAsync(UploadMaterialDto dto, IFormFile file);
    Task<List<MaterialDto>> GetMaterialsAsync(int offeringId);
    Task<byte[]> DownloadMaterialAsync(int id);
}
```

#### 11.3 Create Materials MVC Controller
- Upload files
- Organize in folders
- Link to offerings
- Download/view

#### 11.4 Create Materials API Controller
- GET /api/materials/offering/{id}
- GET /api/materials/download/{id}

#### 11.5 Test Materials
- Upload PDF, DOCX, PPTX
- Create folders
- Download files
- Test via API

---

### DAY 24-25: REPORTS & DASHBOARD

**Tasks**:

#### 12.1 Implement ReportsController
- Attendance report (by offering, date range)
- Grade report (by offering)
- Student transcript
- Export to Excel/PDF

#### 12.2 Create Dashboard
```csharp
[Area("Admin")]
public class DashboardController : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalUsers = await _userService.GetCountAsync(),
            TotalStudents = await _studentService.GetCountAsync(),
            TotalTeachers = await _teacherService.GetCountAsync(),
            ActiveSessionsToday = await _sessionService.GetTodaysCountAsync(),
            OverallAttendanceRate = await _attendanceService.GetTodaysRateAsync(),
            RecentActivity = await _activityService.GetRecentAsync(10)
        };

        return View(model);
    }
}
```

#### 12.3 Create Dashboard View
- Statistics cards
- Charts (attendance trend, grade distribution)
- Recent activity feed
- Quick actions

---

### DAY 26-27: TESTING & BUG FIXES

**Tasks**:

#### 13.1 Unit Testing
- Repository tests
- Service tests (especially algorithms)
- Timetable generation tests
- Auto-grading tests
- GPA calculation tests

#### 13.2 Integration Testing
- API endpoint tests
- Authentication flow tests
- Complete user journey tests

#### 13.3 Manual Testing
**Test Scenarios:**

1. **Admin Flow**:
   - Create users (bulk import)
   - Create departments/courses
   - Generate timetable
   - Review conflicts
   - Publish timetable
   - Create quizzes
   - Configure grades
   - Enter grades
   - View reports

2. **Teacher Flow**:
   - Login
   - View assigned courses
   - Create quiz
   - Assign quiz
   - View submissions
   - Enter grades
   - View attendance

3. **Student Flow (API)**:
   - Login
   - View today's schedule
   - Check in with face
   - Take quiz
   - View grades
   - View GPA
   - Download transcript

#### 13.4 Bug Fixes
- Fix all discovered bugs
- Improve performance
- Optimize queries
- Handle edge cases

---

### DAY 28-29: DEPLOYMENT PREPARATION

**Tasks**:

#### 14.1 Production Configuration
- Update appsettings.Production.json
- Configure connection strings
- Set up logging (Serilog)
- Configure CORS properly
- Set secure JWT secret key

#### 14.2 Database Migration
```bash
# Generate SQL script for production
dotnet ef migrations script --idempotent --output deploy.sql

# OR apply directly
dotnet ef database update --configuration Release
```

#### 14.3 Publish Application
```bash
dotnet publish -c Release -o ./publish
```

#### 14.4 Server Setup
- Install .NET 8 Runtime
- Install SQL Server
- Configure IIS/Nginx
- Set up SSL certificate
- Configure firewall

#### 14.5 Deploy
- Upload published files
- Configure application pool (IIS)
- Test endpoints
- Monitor logs

---

### DAY 30: FINAL TESTING & HANDOVER

**Tasks**:

#### 15.1 Production Testing
- Test all features in production
- Load testing (if needed)
- Security check
- Performance monitoring

#### 15.2 Documentation
- API documentation (Swagger)
- Admin user manual
- Teacher guide
- Student guide (mobile app)
- Deployment guide

#### 15.3 Training
- Admin training session
- Teacher training session
- Q&A

#### 15.4 Handover
- Provide source code
- Provide documentation
- Provide credentials
- Support plan

---

## FINAL CHECKLIST

### System Must Have:
- [ ] ✅ Database created with all tables
- [ ] ✅ Identity Framework configured
- [ ] ✅ User management working (CRUD + bulk import)
- [ ] ✅ Face photo upload & embedding storage
- [ ] ✅ Academic structure complete
- [ ] ✅ Course management working
- [ ] ✅ **Timetable generation working (no conflicts)**
- [ ] ✅ Sessions created from timetable
- [ ] ✅ **Quiz system with auto-grading**
- [ ] ✅ **Face recognition check-in working**
- [ ] ✅ **Grading system with GPA calculation**
- [ ] ✅ Materials upload/download
- [ ] ✅ Reports generation
- [ ] ✅ Admin dashboard
- [ ] ✅ All API endpoints documented
- [ ] ✅ System deployed to production
- [ ] ✅ All tests passing

### Performance Metrics:
- [ ] API response time < 200ms
- [ ] Face recognition < 2 seconds
- [ ] Timetable generation < 30 seconds
- [ ] Page load < 1 second

### Security Checklist:
- [ ] JWT authentication working
- [ ] Role-based authorization enforced
- [ ] Password hashing (Identity)
- [ ] Input validation everywhere
- [ ] SQL injection prevented (EF Core)
- [ ] XSS prevented
- [ ] CORS configured properly

---

## ESTIMATION SUMMARY

| Week | Days | Focus Area | Estimated Hours |
|------|------|------------|-----------------|
| Week 1 | 1-7 | Foundation & Auth | 168 hours |
| Week 2 | 8-14 | Scheduling & Attendance | 192 hours |
| Week 3 | 15-21 | Quizzes & Grading | 216 hours |
| Week 4 | 22-30 | Polish & Deploy | 120 hours |
| **Total** | **30 days** | **Full System** | **696 hours** |

---

**CRITICAL SUCCESS FACTORS**:

1. **Follow Order**: Complete tasks sequentially
2. **Test Each Week**: Don't skip testing
3. **No Parallel Work**: One thing at a time
4. **Fix Before Proceeding**: Resolve issues immediately
5. **Use Claude AI**: Follow prompting guidelines (FILE 5)

---

**END OF FILE 3**
