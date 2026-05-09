# FILE 5: CLAUDE AI PROMPTING INSTRUCTIONS
## SmartClassRoom-LetUNO - Week-by-Week AI Assistance Guide

**Version**: 1.0  
**Date**: December 4, 2025  
**Purpose**: Structured prompts for Claude AI to assist with implementation

---

## HOW TO USE THIS FILE

### General Rules:
1. **Copy prompts exactly** - Don't modify the structure
2. **Provide context** - Always mention project name and tech stack
3. **Be specific** - Reference file names and class names
4. **Request full code** - Ask for complete implementations, not snippets
5. **Iterate** - If result isn't perfect, ask Claude to refine

### Prompt Template Structure:
```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, Identity Framework, EF Core, MS SQL Server

Context: [What you're working on]

Request: [What you need]

Requirements:
- [Specific requirement 1]
- [Specific requirement 2]
- [etc.]

Please provide: [What format you want]
```

---

## WEEK 1: FOUNDATION & CORE SETUP

### DAY 1-2: DATABASE & AUTHENTICATION

#### Prompt 1.1: Create ApplicationDbContext

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, Identity Framework, EF Core, MS SQL Server

Context: I'm creating the main DbContext for my smart classroom management system. I have all POCO classes defined (ApplicationUser, ApplicationRole, Student, Teacher, Department, Level, Section, Group, Course, Term, CourseOffering, StudentEnrollment, Timetable, ScheduledSlot, Session, Room, Attendance, ESPDevice, Quiz, QuizQuestion, QuizAssignment, QuizAttempt, Grade, GradeComponent, MaterialFolder, Material).

Request: Create the ApplicationDbContext class that inherits from IdentityDbContext<ApplicationUser, ApplicationRole, int>.

Requirements:
- Include DbSet for ALL entity classes
- Configure Identity tables
- Configure all entity relationships using Fluent API in OnModelCreating
- Add indexes for commonly queried fields (email, name, date fields)
- Configure cascade delete behavior appropriately
- Use int as primary key for all tables
- Student.FaceEmbeddingBase64 should be nvarchar(max)
- Attendance.Location should be varchar(100)

Please provide: Complete ApplicationDbContext.cs file ready to use
```

#### Prompt 1.2: Create IdentitySeeder

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, Identity Framework, EF Core, MS SQL Server

Context: I need to seed initial roles and admin user when the application starts.

Request: Create IdentitySeeder class with SeedAsync method.

Requirements:
- Seed 3 roles: Admin, Teacher, Student
- Create default admin user: admin@smartclassroom.com / Admin@123
- Assign Admin role to admin user
- Check if roles/user already exist before creating
- Use UserManager<ApplicationUser> and RoleManager<ApplicationRole>
- Make method async and static
- Return void (or Task)

Please provide: Complete IdentitySeeder.cs file
```

#### Prompt 1.3: Configure Program.cs

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, Identity Framework, EF Core, MS SQL Server

Context: I need to configure Program.cs with all necessary services.

Request: Create Program.cs configuration.

Requirements:
- Configure DbContext with SQL Server connection string
- Configure Identity Framework with ApplicationUser and ApplicationRole
- Configure JWT authentication with these settings:
  * Secret key from appsettings
  * Issuer and Audience from appsettings
  * Token expiry 60 minutes
- Add authorization policies:
  * AdminOnly: Admin role required
  * TeacherOnly: Teacher role required
  * StudentOnly: Student role required
  * TeacherOrAdmin: Teacher or Admin roles
  * AllUsers: All 3 roles
- Register repositories (IRepository<T>, IUserRepository, etc.) as Scoped
- Register services (IAuthService, IUserService, etc.) as Scoped
- Configure CORS for mobile app
- Add controllers with views
- Add Swagger for API documentation with JWT Bearer support
- Call IdentitySeeder in startup
- Apply pending migrations on startup

Please provide: Complete Program.cs file
```

---

### DAY 3-4: AUTHENTICATION & IDENTITY

#### Prompt 1.4: Create AuthService

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, Identity Framework, JWT

Context: I need authentication service for login and password management.

Request: Create IAuthService interface and AuthService implementation.

Requirements:
- LoginAsync method:
  * Accept email and password
  * Use SignInManager to validate credentials
  * Generate JWT token with these claims: UserId, Email, FullName, Role
  * Return LoginResponseDto with token, user info, roles
  * Handle invalid credentials
  * Handle account lockout
- ChangePasswordAsync method:
  * Accept userId, oldPassword, newPassword
  * Use UserManager to change password
  * Return success/failure
- RefreshTokenAsync method (optional):
  * Accept refresh token
  * Validate and generate new JWT
- Use IConfiguration to read JWT settings
- Token should expire in 60 minutes

Please provide:
1. IAuthService.cs interface
2. AuthService.cs implementation
3. LoginResponseDto.cs
4. Required DTO classes
```

#### Prompt 1.5: Create Auth API Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0 Web API

Context: I need API controller for authentication endpoints.

Request: Create AuthController for mobile app authentication.

Requirements:
- Route: [Route("api/[controller]")]
- Endpoints:
  * POST /api/auth/login - Accept LoginRequestDto, return LoginResponseDto
  * POST /api/auth/change-password - Accept ChangePasswordDto (requires auth)
  * POST /api/auth/refresh (optional)
- Use [Authorize] attribute where needed
- Return proper HTTP status codes (200, 400, 401, 500)
- Handle exceptions and log errors
- Use IAuthService injected via constructor
- Return JSON responses

Please provide: Complete AuthController.cs
```

---

### DAY 5-7: USER MANAGEMENT

#### Prompt 1.6: Create Generic Repository

```
Project: SmartClassRoom-LetUNO
Tech Stack: EF Core, Repository Pattern

Context: I need generic repository pattern for data access.

Request: Create IRepository<T> interface and Repository<T> implementation.

Requirements:
- Methods:
  * GetByIdAsync(int id)
  * GetAllAsync()
  * FindAsync(Expression<Func<T, bool>> predicate)
  * FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
  * AddAsync(T entity)
  * AddRangeAsync(IEnumerable<T> entities)
  * UpdateAsync(T entity)
  * DeleteAsync(T entity)
  * DeleteRangeAsync(IEnumerable<T> entities)
  * Query() - returns IQueryable<T>
  * GetPagedAsync(pageNumber, pageSize, filter, orderBy)
- All methods async
- Use DbContext injected in constructor
- Repository<T> should work with any entity class

Please provide:
1. IRepository.cs interface
2. Repository.cs implementation
```

#### Prompt 1.7: Create UserService

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core Identity Framework

Context: I need service for user management operations.

Request: Create IUserService interface and UserService implementation.

Requirements:
- Methods:
  * CreateUserAsync(CreateUserViewModel model):
    - Create ApplicationUser
    - Create Student or Teacher based on userType
    - Assign role
    - Link Student/Teacher to ApplicationUser
    - Generate default password
    - Send email (optional)
  * UpdateUserAsync(int id, EditUserViewModel model)
  * DeleteUserAsync(int id)
  * GetAllUsersAsync() - return UserResponseDto list
  * GetUserByIdAsync(int id)
  * BulkImportUsersAsync(IFormFile excelFile):
    - Use EPPlus to parse Excel
    - Validate data
    - Create users in bulk
    - Return (successCount, failCount, errors)
  * UpdateFaceEmbeddingAsync(int userId, string base64Embedding)
  * GetFaceEmbeddingAsync(int userId)
- Use UserManager<ApplicationUser>
- Use IUserRepository, IStudentRepository, ITeacherRepository
- Handle validation
- Handle errors gracefully

Please provide:
1. IUserService.cs interface
2. UserService.cs implementation
3. Required ViewModels
4. Required DTOs
```

#### Prompt 1.8: Create Users MVC Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC, Razor Views

Context: I need admin dashboard controller for user management.

Request: Create UsersController in Admin area.

Requirements:
- [Area("Admin")]
- [Authorize(Roles = "Admin")]
- Actions:
  * Index() - List all users with pagination, search, filter
  * Create() - GET - Display create form
  * Create(CreateUserViewModel) - POST - Create user
  * Edit(int id) - GET - Display edit form
  * Edit(int id, EditUserViewModel) - POST - Update user
  * Delete(int id) - POST - Delete user
  * BulkImport() - GET - Display import form
  * BulkImport(IFormFile) - POST - Process Excel import
  * UploadFacePhoto(int id, IFormFile) - POST - Upload face photo
- Use IUserService
- Return appropriate views
- Handle validation errors
- Display success/error messages (TempData)

Please provide: Complete UsersController.cs
```

#### Prompt 1.9: Create Face Recognition Service Wrapper

```
Project: SmartClassRoom-LetUNO
Tech Stack: Custom Face Recognition Package (your AI model)

Context: I have a custom face recognition package that I need to integrate. It can:
- Get face embedding from image
- Compare two embeddings
- Return similarity score

Request: Create IFaceRecognitionService interface and FaceRecognitionService implementation as a wrapper.

Requirements:
- Methods:
  * GetEmbeddingBase64Async(byte[] imageBytes):
    - Call your face recognition package
    - Get embedding (float array)
    - Convert to Base64 string
    - Return Base64 string
  * VerifyFaceAsync(byte[] imageBytes, string storedEmbeddingBase64):
    - Get embedding from imageBytes
    - Decode storedEmbeddingBase64 back to float array
    - Call your package to compare embeddings
    - Return true if similarity > 0.6 (threshold)
  * RecognizeFaceAsync(byte[] imageBytes, List<string> candidateEmbeddings):
    - Compare against multiple stored embeddings
    - Return best match if similarity > 0.6
- Handle exceptions
- Log errors

Please provide:
1. IFaceRecognitionService.cs interface
2. FaceRecognitionService.cs implementation (with TODO comments for package calls)
3. FaceMatchResult.cs model
```

---

## WEEK 2: SCHEDULING & ATTENDANCE

### DAY 8-9: ACADEMIC STRUCTURE

#### Prompt 2.1: Create Academic Structure Services

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core, EF Core

Context: I need services for managing academic structure (departments, levels, sections, groups).

Request: Create services for Department, Level, Section, and Group management.

Requirements:
For each service (DepartmentService, LevelService, SectionService, GroupService):
- CRUD operations (Create, Read, Update, Delete)
- GetAll with pagination
- Search/filter capability
- Hierarchy validation (e.g., Level must belong to Department)
- Cascade considerations (e.g., deleting Department affects Levels)
- Use corresponding repositories
- Return appropriate DTOs/ViewModels
- Repositories if Needed

Please provide:
1. IDepartmentService.cs + DepartmentService.cs
2. ILevelService.cs + LevelService.cs
3. ISectionService.cs + SectionService.cs
4. IGroupService.cs + GroupService.cs
5. Required ViewModels/DTOs
```
DONEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEe

#### Prompt 2.2: Create Academic Structure MVC Controllers

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC

Context: I need admin dashboard controllers for academic structure management.

Request: Create MVC controllers for Departments, Levels, and Sections.

Requirements:
For each controller (DepartmentsController, LevelsController, SectionsController):
- [Area("Admin")]
- [Authorize(Roles = "Admin")]
- Standard CRUD actions (Index, Create GET/POST, Edit GET/POST, Delete POST)
- Use corresponding service
- Handle parent-child relationships (dropdown selections)
- Display hierarchical data
- Validation and error handling

Please provide:
1. DepartmentsController.cs
2. LevelsController.cs
3. SectionsController.cs
```
doneeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
---

### DAY 10-11: COURSE MANAGEMENT

#### Prompt 2.3: Create Course Services

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core, EF Core

Context: I need services for course catalog, terms, offerings, and enrollments.

Request: Create CourseService, TermService, and OfferingService.

Requirements:
- CourseService:
  * CRUD for courses
  * Handle prerequisites (store as comma-separated IDs)
  * GetPrerequisites(courseId) - return list of prerequisite courses
  * ValidatePrerequisites(studentId, courseId) - check if student completed prerequisites
- TermService:
  * CRUD for academic terms
  * GetCurrentTerm()
  * GetUpcomingTerm()
- OfferingService:
  * Create offering (Course + Term + Teacher + Section/Group + Schedule)
  * EnrollStudent(offeringId, studentId)
  * GetStudentEnrollments(studentId, termId)
  * GetOfferingStudents(offeringId)
  * ValidateEnrollment (check prerequisites, capacity, conflicts)
- And Repositories if needed

Please provide:
1. ICourseService.cs + CourseService.cs
2. ITermService.cs + TermService.cs
3. IOfferingService.cs + OfferingService.cs
4. Required DTOs/ViewModels
```
DONEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
---

### DAY 12-13: TIMETABLE GENERATION

#### Prompt 2.4: Create Timetable Generation Algorithm

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core

Context: I need automatic timetable generation with conflict detection.

Request: Create ITimetableService and implement TimetableService with generation algorithm.

Requirements:
- GenerateTimetableAsync(GenerateTimetableRequest request):
  * Input: DepartmentId, LevelId, SectionId, TermId
  * Get all CourseOfferings for that section/term
  * Define time slots:
    - Sunday-Thursday: 8:00-9:00, 9:00-10:00, ..., 4:00-5:00 (9 slots)
    - 1 slot = 50 minutes
  * For each offering (sorted by sessionsPerWeek DESC):
    - Find available time slots (no teacher conflict, no section conflict)
    - Find suitable room (capacity >= students count, type matches course type)
    - Create ScheduledSlot
  * Return TimetableGenerationResult with:
    - Created timetable ID
    - List of scheduled slots
    - List of conflicts (if any)
    - Success status
- DetectConflictsAsync(timetableId):
  * Check teacher double-booking
  * Check section double-booking
  * Check room double-booking
  * Return list of Conflict objects
- PublishTimetableAsync(timetableId):
  * Create Session entities from ScheduledSlots
  * Set Timetable.IsPublished = true
  * Return success/failure
- REPOSITORIES AND SERVICES IF NEEDED
Please provide:
1. ITimetableService.cs interface
2. TimetableService.cs implementation with COMPLETE algorithm
3. GenerateTimetableRequest.cs
4. TimetableGenerationResult.cs
5. Conflict.cs model
I Need the apility to make custom time table also but the most importanat is to hanlde confilcts between courses offerring and teatchers and sessions and rooms and all of that
```
tobeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee

#### Prompt 2.5: Create Timetable MVC Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC

Context: I need admin interface for timetable generation and review.

Request: Create TimetableController in Admin area.

Requirements:
- Actions:
  * Generate() - GET - Display form (select dept, level, section, term)
  * Generate(GenerateViewModel) - POST - Call TimetableService.GenerateTimetableAsync
  * Review(int timetableId) - GET - Display generated timetable in grid format
  * Publish(int timetableId) - POST - Call TimetableService.PublishTimetableAsync
  * Index() - GET - List all timetables
  * Delete(int id) - POST - Delete unpublished timetable
- Display conflicts prominently on Review page
- Use calendar/grid view for timetable display
- Color-code by course/teacher

Please provide: Complete TimetableController.cs with view models
```

---
to beeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
### DAY 14: SESSION & ATTENDANCE SETUP

#### Prompt 2.6: Create Session and Attendance Services

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core

Context: I need services for session management and attendance tracking.

Request: Create ISessionService and IAttendanceService implementations.

Requirements:
- SessionService:
  * GetTodaysSessionsAsync(userId, role):
    - If role = "Student": return student's scheduled sessions for today
    - If role = "Teacher": return teacher's sessions for today
    - Include offering, course, room info
  * GetSessionDetailAsync(sessionId):
    - Return full session info
    - Include student list with attendance status
  * StartSessionAsync(sessionId):
    - Set Session.ActualStartTime = DateTime.Now
    - Set Session.Status = "InProgress"
  * EndSessionAsync(sessionId):
    - Set Session.ActualEndTime = DateTime.Now
    - Set Session.Status = "Completed"
- AttendanceService:
  * CheckInAsync(sessionId, studentId, faceImage, latitude, longitude):
    - Validate session is active (within 15 mins before start time)
    - Call FaceRecognitionService.VerifyFaceAsync
    - If face matches: create Attendance record with CheckInTime, Location
    - Return CheckInResult (success, message, attendanceId)
  * GetSessionAttendanceAsync(sessionId):
    - Return list of all students with attendance status
  * GetStudentAttendanceAsync(studentId, offeringId):
    - Return attendance summary (total sessions, attended, percentage)

Please provide:
1. ISessionService.cs + SessionService.cs
2. IAttendanceService.cs + AttendanceService.cs
3. Required DTOs
```

#### Prompt 2.7: Create Sessions and Attendance API Controllers

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core Web API

Context: I need API endpoints for mobile app (student session viewing and check-in).

Request: Create SessionsController and AttendanceController for API.

Requirements:
- SessionsController:
  * GET /api/sessions/today - Return today's sessions for authenticated user
  * GET /api/sessions/{id} - Return session details
- AttendanceController:
  * POST /api/attendance/check-in - Accept CheckInRequestDto with:
    - sessionId
    - faceImage (IFormFile or Base64)
    - latitude, longitude
  * GET /api/attendance/my-attendance/{offeringId} - Return student's attendance summary
- Use [Authorize] attribute
- Get userId from ClaimsPrincipal
- Handle file upload for face image
- Return proper status codes

Please provide:
1. SessionsController.cs (API)
2. AttendanceController.cs (API)
3. CheckInRequestDto.cs
4. CheckInResult.cs
```

---

## WEEK 3: ASSESSMENT & GRADING

### DAY 15-16: QUIZ SYSTEM

#### Prompt 3.1: Create Quiz Service with Auto-Grading

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core

Context: I need quiz system with multiple-choice questions and automatic grading.

Request: Create IQuizService and QuizService implementation.

Requirements:
- CreateQuizAsync(CreateQuizViewModel):
  * Create Quiz with title, description, duration, passing score
  * Create QuizQuestions (store as JSON in single field OR separate table)
  * Each question has: text, 4 options, correct answer (1-4), points
  * Return quiz ID
- AssignQuizAsync(quizId, AssignQuizViewModel):
  * Create QuizAssignment for CourseOffering
  * Set start date, end date, attempts allowed
- StartQuizAsync(assignmentId, studentId):
  * Validate: assignment is active, student hasn't exceeded attempts
  * Create QuizAttempt with StartTime
  * Return quiz questions (WITHOUT correct answers)
- SubmitQuizAsync(attemptId, Dictionary<int, int> answers):
  * Save answers as JSON in QuizAttempt.Answers
  * Set EndTime
  * Call GradeQuizAsync
  * Return quiz result
- GradeQuizAsync(attemptId):
  * Get QuizAttempt with Quiz and Questions
  * Parse student answers from JSON
  * Compare with correct answers
  * Calculate score = sum of points for correct answers
  * Calculate percentage = (score / total points) * 100
  * Set Status = "Passed" if percentage >= passing score, else "Failed"
  * Save QuizAttempt with score, percentage, status
  * Return QuizResultDto

Please provide:
1. IQuizService.cs interface
2. QuizService.cs implementation with COMPLETE auto-grading algorithm
3. Required ViewModels/DTOs
```

#### Prompt 3.2: Create Quiz MVC and API Controllers

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC + Web API

Context: I need controllers for quiz creation (teacher) and quiz taking (student).

Request: Create QuizzesController for both MVC (admin) and API (mobile).

Requirements:
- MVC Controller (Admin Area):
  * Create() - GET/POST - Create quiz with questions
  * Assign(quizId) - GET/POST - Assign to offering
  * Submissions(assignmentId) - GET - View all student submissions
  * Index() - GET - List all quizzes
- API Controller:
  * GET /api/quizzes/available - Student's available quiz assignments
  * POST /api/quizzes/start/{assignmentId} - Start quiz attempt
  * POST /api/quizzes/submit - Submit quiz answers
  * GET /api/quizzes/results/{attemptId} - View quiz results
- Handle validation
- Use IQuizService

Please provide:
1. QuizzesController.cs (MVC in Admin area)
2. QuizzesController.cs (API)
3. Required ViewModels/DTOs
```

---

### DAY 17-18: FACE RECOGNITION ATTENDANCE

#### Prompt 3.3: Enhance AttendanceService with Face Recognition

```
Project: SmartClassRoom-LetUNO
Tech Stack: Custom Face Recognition Package

Context: I already have AttendanceService with CheckInAsync method. Now I need to integrate face recognition validation.

Request: Enhance CheckInAsync method to use face recognition.

Requirements:
- CheckInAsync(sessionId, studentId, byte[] faceImageBytes, decimal latitude, decimal longitude):
  * Validate session exists and is active (within 15 mins before scheduled time)
  * Validate student is enrolled in offering
  * Get student's stored face embedding (ApplicationUser.FaceEmbeddingBase64)
  * Call IFaceRecognitionService.VerifyFaceAsync(faceImageBytes, storedEmbedding)
  * If face verification fails: return error "Face verification failed"
  * Validate location (optional: check if lat/lng within campus radius)
  * Create Attendance record:
    - SessionId, StudentId
    - CheckInTime = DateTime.Now
    - Location = "lat,lng"
    - VerificationMethod = "FaceRecognition"
  * Return CheckInResult with success, message, attendanceId

Please provide: Enhanced AttendanceService.cs with complete CheckInAsync implementation
```

---

### DAY 19-21: GRADING SYSTEM & GPA

#### Prompt 3.4: Create Grading Service with GPA Calculation

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core

Context: I need comprehensive grading system with final grade calculation and GPA.

Request: Create IGradingService and GradingService implementation.

Requirements:
- SaveGradeComponentsAsync(offeringId, List<GradeComponentDto>):
  * Save grade components for offering (e.g., Midterm 30%, Final 40%, Assignment 20%, Attendance 10%)
  * Validate weights sum to 100%
- SaveGradeAsync(SaveGradeDto):
  * Save grade for specific component, student, offering
  * Score should be 0-100
- CalculateFinalGradesAsync(offeringId):
  * For each enrolled student:
    - Get all component grades
    - Calculate final score = Σ(componentScore × componentWeight)
    - Convert to letter grade using scale:
      * A:  90-100% → 4.0
      * B+: 85-89%  → 3.3
      * B:  80-84%  → 3.0
      * C+: 75-79%  → 2.3
      * C:  70-74%  → 2.0
      * D:  60-69%  → 1.0
      * F:  <60%    → 0.0
    - Save Grade record with FinalScore, LetterGrade, GradePoint
  * Return CalculationResult
- CalculateGPAAsync(studentId, termId):
  * Get all completed courses for student (optionally filter by term)
  * Calculate GPA = Σ(GradePoint × Credits) / Σ(Credits)
  * Example: Course1(A/4.0 × 3) + Course2(B+/3.3 × 3) + Course3(A-/3.7 × 4) = 36.7 / 10 = 3.67
  * Return GPADto with term GPA, cumulative GPA, total credits
- GenerateTranscriptPdfAsync(studentId):
  * Get all courses with grades
  * Generate PDF with student info, course list, grades, GPA
  * Use iTextSharp or similar library
  * Return byte[] of PDF

Please provide:
1. IGradingService.cs interface
2. GradingService.cs implementation with COMPLETE algorithms
3. Required DTOs
```

#### Prompt 3.5: Create Grades MVC and API Controllers

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC + Web API

Context: I need controllers for grade management (teacher/admin) and grade viewing (student).

Request: Create GradesController for both MVC and API.

Requirements:
- MVC Controller (Admin Area):
  * ConfigureComponents(offeringId) - GET - Display component configuration form
  * SaveComponents(ComponentsViewModel) - POST - Save components
  * GradeSheet(offeringId) - GET - Display grade entry sheet (Excel-like)
  * SaveGrade(SaveGradeDto) - POST - Save individual grade (AJAX)
  * CalculateFinalGrades(offeringId) - POST - Trigger final grade calculation
- API Controller:
  * GET /api/grades/my-courses - Student's enrolled courses with grades
  * GET /api/grades/course/{offeringId} - Detailed grade breakdown for course
  * GET /api/grades/gpa - Student's GPA (term and cumulative)
  * GET /api/grades/transcript - Download transcript PDF

Please provide:
1. GradesController.cs (MVC in Admin area)
2. GradesController.cs (API)
3. Required ViewModels/DTOs
```

---

## WEEK 4: MATERIALS, REPORTS & DEPLOYMENT

### DAY 22-23: MATERIALS MANAGEMENT

#### Prompt 4.1: Create Material Service

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core, File Storage

Context: I need file upload/download system for course materials.

Request: Create IMaterialService and MaterialService implementation.

Requirements:
- CreateFolderAsync(CreateFolderDto):
  * Create MaterialFolder for offering
  * Parent folder support (nested folders)
- UploadMaterialAsync(UploadMaterialDto, IFormFile file):
  * Validate file size (max 10MB)
  * Validate file type (PDF, DOCX, PPTX, XLSX, images)
  * Save file to wwwroot/uploads/materials/{offeringId}/{filename}
  * Create Material record with:
    - Name, FileType, FileSize, FilePath, UploadedById, UploadedDate
  * Return material ID
- GetMaterialsAsync(offeringId):
  * Return hierarchical list of folders and materials
- DownloadMaterialAsync(materialId):
  * Get Material record
  * Read file from FilePath
  * Return byte[] with content type
- DeleteMaterialAsync(materialId):
  * Delete file from disk
  * Delete Material record

Please provide:
1. IMaterialService.cs interface
2. MaterialService.cs implementation
3. IFileStorageService.cs + FileStorageService.cs (helper for file operations)
4. Required DTOs
```

#### Prompt 4.2: Create Materials API Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core Web API

Context: I need API endpoints for material viewing and downloading.

Request: Create MaterialsController for API.

Requirements:
- GET /api/materials/offering/{offeringId} - List materials for offering
- GET /api/materials/download/{materialId} - Download file
  * Return File(bytes, contentType, filename)
- POST /api/materials/upload - Upload file (teacher only)
- [Authorize] attribute on all actions
- Role-based access: teachers can upload, students can only view/download

Please provide: MaterialsController.cs (API)
```

---

### DAY 24-25: REPORTS & DASHBOARD

#### Prompt 4.3: Create Reports Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC, EPPlus (Excel), iTextSharp (PDF)

Context: I need reporting functionality for admin/teachers.

Request: Create ReportsController in Admin area.

Requirements:
- Actions:
  * AttendanceReport(offeringId, dateFrom, dateTo) - GET:
    - Generate attendance report (Excel or PDF)
    - Include: student list, session dates, attendance status
    - Calculate attendance percentage per student
  * GradeReport(offeringId) - GET:
    - Generate grade report (Excel or PDF)
    - Include: student list, component grades, final grade, letter grade
    - Calculate class average
  * StudentTranscript(studentId) - GET:
    - Generate official transcript (PDF)
    - Include: student info, all courses, grades, term GPA, cumulative GPA
  * Export formats: Excel (XLSX), PDF
- Use EPPlus for Excel generation
- Use iTextSharp for PDF generation
- Return File() result with appropriate content type

Please provide: Complete ReportsController.cs with report generation logic
```

#### Prompt 4.4: Create Dashboard Controller

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core MVC

Context: I need admin dashboard with statistics and quick actions.

Request: Create DashboardController in Admin area.

Requirements:
- Index() action:
  * Display statistics:
    - Total users (students, teachers, admins)
    - Active sessions today
    - Overall attendance rate today
    - Upcoming quizzes
    - Recent activity log
  * Display charts:
    - Attendance trend (last 7 days)
    - Grade distribution (current term)
  * Quick actions:
    - Create user
    - Generate timetable
    - Create quiz
- Use Chart.js or similar for charts
- Use services to fetch data

Please provide:
1. DashboardController.cs
2. DashboardViewModel.cs
3. Sample Index.cshtml structure (Razor view)
```

---

### DAY 26-30: TESTING & DEPLOYMENT

#### Prompt 4.5: Create Unit Tests

```
Project: SmartClassRoom-LetUNO
Tech Stack: xUnit, Moq

Context: I need unit tests for critical services.

Request: Create unit test project with tests for TimetableService, QuizService, and GradingService.

Requirements:
- Create SmartClassRoom.Tests project (xUnit)
- Mock dependencies using Moq
- Test cases for TimetableService:
  * GenerateTimetableAsync should create slots without conflicts
  * DetectConflictsAsync should identify teacher conflicts
  * DetectConflictsAsync should identify room conflicts
- Test cases for QuizService:
  * GradeQuizAsync should calculate score correctly
  * GradeQuizAsync should set correct status (Pass/Fail)
  * Auto-grading should handle all questions
- Test cases for GradingService:
  * CalculateFinalGradesAsync should calculate weighted average
  * Letter grade conversion should be accurate
  * GPA calculation should be accurate
- Use [Fact] and [Theory] attributes
- Arrange-Act-Assert pattern

Please provide:
1. SmartClassRoom.Tests.csproj file
2. TimetableServiceTests.cs
3. QuizServiceTests.cs
4. GradingServiceTests.cs
```

#### Prompt 4.6: Create Deployment Guide

```
Project: SmartClassRoom-LetUNO
Tech Stack: ASP.NET Core 8.0, MS SQL Server, IIS

Context: I need step-by-step deployment guide for production server.

Request: Create comprehensive deployment documentation.

Requirements:
Include steps for:
1. Server prerequisites:
   - Install .NET 8 Runtime
   - Install SQL Server 2022
   - Install IIS with ASP.NET Core Module
2. Database deployment:
   - Generate SQL migration script
   - Execute on production database
   - Verify tables created
3. Application deployment:
   - Publish application (Release configuration)
   - Copy files to server
   - Configure IIS application pool
   - Configure IIS website
4. Configuration:
   - Update appsettings.Production.json
   - Set connection strings
   - Configure JWT settings
   - Set up SSL certificate
5. Post-deployment:
   - Test endpoints
   - Run smoke tests
   - Monitor logs
6. Troubleshooting common issues

Please provide: DEPLOYMENT_GUIDE.md with complete instructions
```

---

## GENERAL TROUBLESHOOTING PROMPTS

### When You Encounter Errors

#### Prompt T.1: Debug Entity Framework Error

```
Project: SmartClassRoom-LetUNO
Tech Stack: Entity Framework Core

Context: I'm getting the following error when running migration/query:
[PASTE ERROR HERE]

My entity configuration:
[PASTE RELEVANT CODE]

Request: Explain the error and provide the fix.

Requirements:
- Explain what's causing the error
- Provide corrected code
- Explain why the fix works
```

#### Prompt T.2: Debug JWT Authentication Error

```
Project: SmartClassRoom-LetUNO
Tech Stack: JWT Authentication

Context: I'm getting 401 Unauthorized when calling API endpoint.

Setup:
- JWT token is generated successfully on login
- Token is included in Authorization header as "Bearer {token}"
- Error occurs on: [ENDPOINT]

Request: Debug and fix the authentication issue.

Requirements:
- Check JWT configuration in Program.cs
- Check token generation in AuthService
- Check [Authorize] attribute usage
- Provide solution
```

#### Prompt T.3: Optimize Slow Query

```
Project: SmartClassRoom-LetUNO
Tech Stack: Entity Framework Core, LINQ

Context: This query is running slowly:
[PASTE LINQ QUERY]

Request: Optimize this query for better performance.

Requirements:
- Identify performance issues (N+1 queries, missing indexes, etc.)
- Provide optimized version
- Suggest database indexes if needed
```

---

## BEST PRACTICES FOR USING CLAUDE

### DO:
✅ Provide full context (project name, tech stack, what you're building)
✅ Share relevant code when asking for modifications
✅ Request complete implementations (not snippets)
✅ Ask for explanation of complex algorithms
✅ Request error handling and validation
✅ Ask for comments in code for clarity
✅ Specify naming conventions you want to follow
✅ Request DTOs/ViewModels alongside service methods

### DON'T:
❌ Ask vague questions like "create user management"
❌ Omit important context
❌ Ask for modifications without showing current code
❌ Accept incomplete implementations
❌ Skip error handling
❌ Forget to mention required packages/libraries
❌ Mix multiple unrelated requests in one prompt

---

## EXAMPLE ITERATION FLOW

### Initial Prompt:
"Create UserService with CreateUserAsync method..."

### Claude Response:
[Provides code]

### Follow-up Prompt:
```
This is good, but please also:
1. Add email validation before creating user
2. Check if email already exists
3. Handle the case where role assignment fails
4. Return more detailed error messages in UserServiceResult

Please provide updated UserService.cs with these improvements.
```

### Claude Response:
[Provides improved code]

### Final Verification:
"Perfect! Now generate the corresponding unit tests for UserService.CreateUserAsync..."

---

## WEEKLY CHECKPOINT PROMPTS

Use these to verify completion at end of each week:

### Week 1 Checkpoint:

```
Project: SmartClassRoom-LetUNO

Context: I've completed Week 1 implementation.

Request: Create a checklist to verify Week 1 is complete.

Week 1 includes:
- Database setup with all tables
- Identity Framework configuration
- Authentication API (login, change password)
- User management CRUD
- Bulk user import from Excel
- Face photo upload and storage

Please provide: A detailed test checklist with test scenarios for each feature.
```

### Week 2-4 Checkpoints:
(Similar format, listing completed features)

---

**END OF FILE 5**

---

## FINAL NOTES

1. **Always specify project name and tech stack** in every prompt
2. **Provide context** - what you've already done, what you're working on now
3. **Request complete code** - full files, not snippets
4. **Iterate** - if first response isn't perfect, ask for refinements
5. **Test as you go** - verify each feature before moving to next
6. **Document** - keep notes on what works and what doesn't

Good luck with your implementation! 🚀
