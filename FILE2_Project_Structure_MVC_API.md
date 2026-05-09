# FILE 2: PROJECT STRUCTURE
## SmartClassRoom-LetUNO - MVC + API Combined Solution

**Version**: 1.0  
**Date**: December 4, 2025  
**Framework**: ASP.NET Core 8.0  
**Architecture**: MVC + Web API (Single Project)  
**Authentication**: ASP.NET Core Identity Framework  
**Database**: MS SQL Server + Entity Framework Core

---

## 1. SOLUTION STRUCTURE

### 1.1 Recommended Approach: Single Project

**Why Single Project?**
- ✅ Simplified deployment (one application)
- ✅ Shared authentication/authorization
- ✅ Single DbContext
- ✅ Easier dependency management
- ✅ MVC dashboard can use API internally
- ✅ Better for small-to-medium projects

```
SmartClassRoom.sln
│
└── SmartClassRoom.Web/                    # Main Project (MVC + API)
    ├── Areas/
    │   ├── Admin/                         # Admin MVC Area
    │   │   ├── Controllers/
    │   │   │   ├── DashboardController.cs
    │   │   │   ├── UsersController.cs
    │   │   │   ├── DepartmentsController.cs
    │   │   │   ├── CoursesController.cs
    │   │   │   ├── OfferingsController.cs
    │   │   │   ├── TimetableController.cs
    │   │   │   ├── SessionsController.cs
    │   │   │   ├── AttendanceController.cs
    │   │   │   ├── QuizzesController.cs
    │   │   │   ├── GradesController.cs
    │   │   │   ├── MaterialsController.cs
    │   │   │   ├── RoomsController.cs
    │   │   │   └── ReportsController.cs
    │   │   └── Views/
    │   │       ├── Dashboard/
    │   │       ├── Users/
    │   │       ├── Departments/
    │   │       ├── Courses/
    │   │       ├── Offerings/
    │   │       ├── Timetable/
    │   │       ├── Sessions/
    │   │       ├── Attendance/
    │   │       ├── Quizzes/
    │   │       ├── Grades/
    │   │       ├── Materials/
    │   │       └── Reports/
    │   │
    │   └── Identity/                      # Identity UI Area (optional override)
    │       └── Pages/
    │           └── Account/
    │
    ├── Controllers/                       # API Controllers
    │   ├── Api/
    │   │   ├── AuthController.cs         # [Route("api/[controller]")]
    │   │   ├── UsersController.cs
    │   │   ├── ScheduleController.cs
    │   │   ├── SessionsController.cs
    │   │   ├── AttendanceController.cs
    │   │   ├── QuizzesController.cs
    │   │   ├── GradesController.cs
    │   │   ├── MaterialsController.cs
    │   │   └── ESPController.cs
    │   │
    │   └── HomeController.cs              # MVC Home Controller
    │
    ├── Models/
    │   ├── Entities/                      # POCO Classes (from File 1)
    │   │   ├── Identity/
    │   │   │   ├── ApplicationUser.cs
    │   │   │   ├── ApplicationRole.cs
    │   │   │   ├── ApplicationUserRole.cs
    │   │   │   └── ApplicationRoleClaim.cs
    │   │   ├── Users/
    │   │   │   ├── Student.cs
    │   │   │   └── Teacher.cs
    │   │   ├── Academic/
    │   │   │   ├── University.cs
    │   │   │   ├── Department.cs
    │   │   │   ├── Level.cs
    │   │   │   ├── Section.cs
    │   │   │   ├── Group.cs
    │   │   │   ├── Course.cs
    │   │   │   ├── Term.cs
    │   │   │   ├── CourseOffering.cs
    │   │   │   └── StudentEnrollment.cs
    │   │   ├── Scheduling/
    │   │   │   ├── Timetable.cs
    │   │   │   ├── ScheduledSlot.cs
    │   │   │   ├── Session.cs
    │   │   │   └── Room.cs
    │   │   ├── Attendance/
    │   │   │   ├── Attendance.cs
    │   │   │   └── ESPDevice.cs
    │   │   ├── Quizzes/
    │   │   │   ├── Quiz.cs
    │   │   │   ├── QuizQuestion.cs
    │   │   │   ├── QuizAssignment.cs
    │   │   │   └── QuizAttempt.cs
    │   │   ├── Grading/
    │   │   │   ├── GradeComponent.cs
    │   │   │   └── Grade.cs
    │   │   └── Materials/
    │   │       ├── MaterialFolder.cs
    │   │       └── Material.cs
    │   │
    │   ├── ViewModels/                    # For MVC Views
    │   │   ├── Account/
    │   │   │   ├── LoginViewModel.cs
    │   │   │   ├── RegisterViewModel.cs
    │   │   │   └── ChangePasswordViewModel.cs
    │   │   ├── Dashboard/
    │   │   │   └── DashboardViewModel.cs
    │   │   ├── Users/
    │   │   │   ├── UserListViewModel.cs
    │   │   │   ├── CreateUserViewModel.cs
    │   │   │   ├── EditUserViewModel.cs
    │   │   │   └── BulkImportViewModel.cs
    │   │   ├── Timetable/
    │   │   │   ├── TimetableGenerateViewModel.cs
    │   │   │   ├── TimetableReviewViewModel.cs
    │   │   │   └── ConflictViewModel.cs
    │   │   ├── Quizzes/
    │   │   │   ├── QuizCreateViewModel.cs
    │   │   │   ├── QuizEditViewModel.cs
    │   │   │   ├── QuizAssignViewModel.cs
    │   │   │   └── QuizSubmissionViewModel.cs
    │   │   └── Grades/
    │   │       ├── GradeConfigViewModel.cs
    │   │       ├── GradeSheetViewModel.cs
    │   │       └── GPAViewModel.cs
    │   │
    │   └── DTOs/                          # For API
    │       ├── Request/
    │       │   ├── LoginRequestDto.cs
    │       │   ├── RegisterRequestDto.cs
    │       │   ├── CheckInRequestDto.cs
    │       │   ├── StartQuizDto.cs
    │       │   ├── SubmitQuizDto.cs
    │       │   └── SaveGradeDto.cs
    │       └── Response/
    │           ├── LoginResponseDto.cs
    │           ├── UserResponseDto.cs
    │           ├── SessionResponseDto.cs
    │           ├── AttendanceResponseDto.cs
    │           ├── QuizResponseDto.cs
    │           ├── QuizResultDto.cs
    │           ├── GradeResponseDto.cs
    │           └── GPAResponseDto.cs
    │
    ├── Data/
    │   ├── ApplicationDbContext.cs        # Main DbContext (from File 1)
    │   ├── Repositories/
    │   │   ├── Interfaces/
    │   │   │   ├── IRepository.cs         # Generic repository interface
    │   │   │   ├── IUserRepository.cs
    │   │   │   ├── IStudentRepository.cs
    │   │   │   ├── ITeacherRepository.cs
    │   │   │   ├── IDepartmentRepository.cs
    │   │   │   ├── ICourseRepository.cs
    │   │   │   ├── IOfferingRepository.cs
    │   │   │   ├── ITimetableRepository.cs
    │   │   │   ├── ISessionRepository.cs
    │   │   │   ├── IAttendanceRepository.cs
    │   │   │   ├── IQuizRepository.cs
    │   │   │   ├── IGradeRepository.cs
    │   │   │   └── IMaterialRepository.cs
    │   │   │
    │   │   └── Implementations/
    │   │       ├── Repository.cs          # Generic repository implementation
    │   │       ├── UserRepository.cs
    │   │       ├── StudentRepository.cs
    │   │       ├── TeacherRepository.cs
    │   │       ├── DepartmentRepository.cs
    │   │       ├── CourseRepository.cs
    │   │       ├── OfferingRepository.cs
    │   │       ├── TimetableRepository.cs
    │   │       ├── SessionRepository.cs
    │   │       ├── AttendanceRepository.cs
    │   │       ├── QuizRepository.cs
    │   │       ├── GradeRepository.cs
    │   │       └── MaterialRepository.cs
    │   │
    │   ├── Migrations/                    # EF Core Migrations
    │   │   └── [Timestamp]_InitialCreate.cs
    │   │
    │   └── Seed/
    │       ├── DataSeeder.cs
    │       └── IdentitySeeder.cs          # Seed roles & admin user
    │
    ├── Services/
    │   ├── Interfaces/
    │   │   ├── IAuthService.cs
    │   │   ├── IUserService.cs
    │   │   ├── IStudentService.cs
    │   │   ├── ITeacherService.cs
    │   │   ├── IDepartmentService.cs
    │   │   ├── ICourseService.cs
    │   │   ├── IOfferingService.cs
    │   │   ├── ITimetableService.cs       # Timetable generation algorithm
    │   │   ├── ISessionService.cs
    │   │   ├── IAttendanceService.cs
    │   │   ├── IQuizService.cs            # Quiz auto-grading
    │   │   ├── IGradingService.cs         # GPA calculation
    │   │   ├── IMaterialService.cs
    │   │   ├── IFaceRecognitionService.cs # Your custom AI package
    │   │   ├── IFileStorageService.cs
    │   │   └── IEmailService.cs
    │   │
    │   └── Implementations/
    │       ├── AuthService.cs
    │       ├── UserService.cs
    │       ├── StudentService.cs
    │       ├── TeacherService.cs
    │       ├── DepartmentService.cs
    │       ├── CourseService.cs
    │       ├── OfferingService.cs
    │       ├── TimetableService.cs
    │       ├── SessionService.cs
    │       ├── AttendanceService.cs
    │       ├── QuizService.cs
    │       ├── GradingService.cs
    │       ├── MaterialService.cs
    │       ├── FaceRecognitionService.cs  # Wrapper for your AI package
    │       ├── FileStorageService.cs
    │       └── EmailService.cs
    │
    ├── Utilities/
    │   ├── Extensions/
    │   │   ├── IdentityExtensions.cs
    │   │   ├── ServiceCollectionExtensions.cs
    │   │   └── ClaimsPrincipalExtensions.cs
    │   ├── Helpers/
    │   │   ├── ExcelHelper.cs
    │   │   ├── PdfHelper.cs
    │   │   └── DateTimeHelper.cs
    │   ├── Validators/
    │   │   ├── UserValidator.cs
    │   │   ├── CourseValidator.cs
    │   │   └── QuizValidator.cs
    │   └── Constants/
    │       ├── Roles.cs                   # Role names constants
    │       ├── Policies.cs                # Authorization policy names
    │       └── AppSettings.cs
    │
    ├── Middleware/
    │   ├── ErrorHandlingMiddleware.cs
    │   ├── RequestLoggingMiddleware.cs
    │   └── PerformanceMonitoringMiddleware.cs
    │
    ├── wwwroot/                           # Static files
    │   ├── css/
    │   │   ├── site.css
    │   │   └── dashboard.css
    │   ├── js/
    │   │   ├── site.js
    │   │   ├── timetable.js
    │   │   └── quiz.js
    │   ├── lib/                           # Client libraries (Bootstrap, jQuery, etc.)
    │   ├── images/
    │   └── uploads/                       # User uploads
    │       ├── faces/                     # Face photos
    │       └── materials/                 # Course materials
    │
    ├── Views/                             # MVC Views (Non-Area)
    │   ├── Home/
    │   │   ├── Index.cshtml
    │   │   └── Privacy.cshtml
    │   ├── Shared/
    │   │   ├── _Layout.cshtml
    │   │   ├── _ValidationScriptsPartial.cshtml
    │   │   ├── _LoginPartial.cshtml
    │   │   ├── Error.cshtml
    │   │   └── Components/
    │   │       └── NavigationMenuViewComponent.cs
    │   └── _ViewImports.cshtml
    │
    ├── Properties/
    │   └── launchSettings.json
    │
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Program.cs                         # Application entry point
    └── SmartClassRoom.Web.csproj         # Project file
```

---

## 2. PROJECT FILE (SmartClassRoom.Web.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Identity Framework -->
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.0" />

    <!-- Entity Framework Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

    <!-- Authentication -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />

    <!-- Excel Operations -->
    <PackageReference Include="EPPlus" Version="7.0.0" />
    <PackageReference Include="ClosedXML" Version="0.102.0" />

    <!-- PDF Generation -->
    <PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.4.0" />

    <!-- API Documentation -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />

    <!-- AutoMapper -->
    <PackageReference Include="AutoMapper" Version="12.0.1" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />

    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />

    <!-- Validation -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

    <!-- HTTP Client for external APIs -->
    <PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />

  </ItemGroup>

</Project>
```

---

## 3. PROGRAM.CS CONFIGURATION

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Services.Interfaces;
using SmartClassRoom.Web.Services.Implementations;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Data.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE CONFIGURATION =====
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ===== IDENTITY FRAMEWORK CONFIGURATION =====
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Change to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Optional: Adds default Identity UI pages

// ===== JWT AUTHENTICATION FOR API =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// ===== AUTHORIZATION POLICIES =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
    options.AddPolicy("AllUsers", policy => policy.RequireRole("Student", "Teacher", "Admin"));
});

// ===== REPOSITORY REGISTRATION =====
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IOfferingRepository, OfferingRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IGradeRepository, GradeRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();

// ===== SERVICE REGISTRATION =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ===== AUTOMAPPER =====
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ===== CORS FOR MOBILE APP =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== MVC & API CONTROLLERS =====
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // For Identity UI

// ===== API DOCUMENTATION (SWAGGER) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SmartClassRoom API",
        Version = "v1",
        Description = "API for SmartClassRoom Mobile App"
    });

    // JWT Bearer Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== BUILD APP =====
var app = builder.Build();

// ===== SEED DATABASE =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // Apply pending migrations
        await context.Database.MigrateAsync();

        // Seed roles and admin user
        await IdentitySeeder.SeedAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartClassRoom API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowMobileApp");

app.UseAuthentication(); // MUST come before UseAuthorization
app.UseAuthorization();

// ===== ROUTE CONFIGURATION =====
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // For Identity UI

app.Run();
```

---

## 4. APPSETTINGS.JSON

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartClassRoom;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },

  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMustBeAtLeast32CharactersLong!ChangeInProduction",
    "Issuer": "SmartClassRoomAPI",
    "Audience": "SmartClassRoomClients",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },

  "FileStorage": {
    "FacePhotosPath": "wwwroot/uploads/faces",
    "MaterialsPath": "wwwroot/uploads/materials",
    "MaxFileSizeMB": 10
  },

  "FaceRecognition": {
    "ConfidenceThreshold": 0.6,
    "MaxDistance": 0.6
  },

  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@smartclassroom.com",
    "SenderName": "SmartClassRoom",
    "UseSsl": true
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },

  "AllowedHosts": "*"
}
```

---

## 5. AREA STRUCTURE (Admin MVC)

### 5.1 Admin Area Registration

**Areas/Admin/_ViewImports.cshtml**

```cshtml
@using SmartClassRoom.Web
@using SmartClassRoom.Web.Models
@using SmartClassRoom.Web.Models.ViewModels
@using SmartClassRoom.Web.Models.ViewModels.Dashboard
@using SmartClassRoom.Web.Models.ViewModels.Users
@using SmartClassRoom.Web.Models.ViewModels.Timetable
@using SmartClassRoom.Web.Models.ViewModels.Quizzes
@using SmartClassRoom.Web.Models.ViewModels.Grades
@using Microsoft.AspNetCore.Identity
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**Areas/Admin/_ViewStart.cshtml**

```cshtml
@{
    Layout = "_Layout";
}
```

### 5.2 Sample Admin Controller Structure

**Areas/Admin/Controllers/DashboardController.cs**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Services.Interfaces;
using SmartClassRoom.Web.Models.ViewModels.Dashboard;

namespace SmartClassRoom.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISessionService _sessionService;
        private readonly IAttendanceService _attendanceService;

        public DashboardController(
            IUserService userService,
            ISessionService sessionService,
            IAttendanceService attendanceService)
        {
            _userService = userService;
            _sessionService = sessionService;
            _attendanceService = attendanceService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                // Populate dashboard statistics
            };

            return View(model);
        }
    }
}
```

---

## 6. API CONTROLLERS STRUCTURE

### 6.1 Sample API Controller

**Controllers/Api/AuthController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Services.Interfaces;
using SmartClassRoom.Web.Models.DTOs.Request;
using SmartClassRoom.Web.Models.DTOs.Response;

namespace SmartClassRoom.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// User login endpoint
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request.Email, request.Password);

                if (!result.Success)
                    return Unauthorized(new { message = result.ErrorMessage });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new { message = "Login failed" });
            }
        }

        /// <summary>
        /// Change password endpoint
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            try
            {
                var userId = User.GetUserId(); // Extension method
                var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

                if (!result.Success)
                    return BadRequest(new { message = result.ErrorMessage });

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change password error");
                return StatusCode(500, new { message = "Password change failed" });
            }
        }
    }
}
```

---

## 7. REPOSITORY PATTERN STRUCTURE

### 7.1 Generic Repository Interface

**Data/Repositories/Interfaces/IRepository.cs**

```csharp
using System.Linq.Expressions;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Read operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // Write operations
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);

        // Queryable
        IQueryable<T> Query();

        // Pagination
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    }
}
```

### 7.2 Generic Repository Implementation

**Data/Repositories/Implementations/Repository.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SmartClassRoom.Web.Data.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            var totalCount = await query.CountAsync();

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
```

---

## 8. SERVICE PATTERN STRUCTURE

### 8.1 Sample Service Interface

**Services/Interfaces/IUserService.cs**

```csharp
using SmartClassRoom.Web.Models.ViewModels.Users;
using SmartClassRoom.Web.Models.DTOs.Response;

namespace SmartClassRoom.Web.Services.Interfaces
{
    public interface IUserService
    {
        // User management
        Task<UserResponseDto> GetUserByIdAsync(int id);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> CreateUserAsync(CreateUserViewModel model);
        Task<bool> UpdateUserAsync(int id, EditUserViewModel model);
        Task<bool> DeleteUserAsync(int id);

        // Face recognition
        Task<bool> UpdateFaceEmbeddingAsync(int userId, string base64Embedding);
        Task<string?> GetFaceEmbeddingAsync(int userId);

        // Bulk operations
        Task<(int Success, int Failed, List<string> Errors)> BulkImportUsersAsync(IFormFile file);

        // Statistics
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetStudentsCountAsync();
        Task<int> GetTeachersCountAsync();
    }
}
```

---

## 9. IDENTITY SEEDER

**Data/Seed/IdentitySeeder.cs**

```csharp
using Microsoft.AspNetCore.Identity;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Models.Entities.Users;

namespace SmartClassRoom.Web.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            // Seed Roles
            var roles = new[] { "Admin", "Teacher", "Student" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role"
                    });
                }
            }

            // Seed Admin User
            var adminEmail = "admin@smartclassroom.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    UserType = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
```

---

## 10. UTILITIES & EXTENSIONS

### 10.1 ClaimsPrincipal Extensions

**Utilities/Extensions/ClaimsPrincipalExtensions.cs**

```csharp
using System.Security.Claims;

namespace SmartClassRoom.Web.Utilities.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public static string? GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetUserRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static bool IsInRole(this ClaimsPrincipal principal, params string[] roles)
        {
            return roles.Any(role => principal.IsInRole(role));
        }
    }
}
```

### 10.2 Role Constants

**Utilities/Constants/Roles.cs**

```csharp
namespace SmartClassRoom.Web.Utilities.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Teacher = "Teacher";
        public const string Student = "Student";

        public static readonly string[] All = { Admin, Teacher, Student };
        public static readonly string[] AdminAndTeacher = { Admin, Teacher };
    }
}
```

---

## 11. SUMMARY

### Project Structure Benefits:
- ✅ **Single Project**: Simplified deployment
- ✅ **MVC + API**: Dashboard + Mobile support
- ✅ **Identity Framework**: Built-in authentication
- ✅ **Repository Pattern**: Clean data access
- ✅ **Service Layer**: Business logic separation
- ✅ **Areas**: Organized admin functionality
- ✅ **DTOs/ViewModels**: Separation of concerns

### Key Folders:
1. **Areas/Admin**: MVC dashboard controllers & views
2. **Controllers/Api**: REST API for mobile app
3. **Models**: Entities, ViewModels, DTOs
4. **Data**: DbContext, Repositories, Migrations
5. **Services**: Business logic layer
6. **Utilities**: Helpers, Extensions, Constants

### Technologies:
- ASP.NET Core 8.0
- Identity Framework (authentication)
- Entity Framework Core (ORM)
- MS SQL Server (database)
- JWT (API authentication)
- Swagger (API documentation)

---

**END OF FILE 2**
