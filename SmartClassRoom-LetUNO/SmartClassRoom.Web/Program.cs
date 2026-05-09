using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using SmartClassRoom.Web.Data;
using SmartClassRoom.Web.Data.Repositories.Implementations;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Data.Seed;
using SmartClassRoom.Web.Models.Entities.Identity;
using SmartClassRoom.Web.Services.Implementations;
using SmartClassRoom.Web.Services.Interfaces;
using SmartClassRoom.Web.Services.BackgroundServices;
using SmartClassRoom.Web.Utilities.Constants;
using System.Text;
using FaceRecognition.Core;
using FaceRecognition.Core.Configuration;
using SmartClassRoom.Web.Configuration;

// Configure QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity Framework
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Configure JWT Authentication for API (Cookie auth is default from AddIdentity)
builder.Services.AddAuthentication()
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? string.Empty)),
        ClockSkew = TimeSpan.Zero
    };
});

// Register Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<IScheduledSlotRepository, ScheduledSlotRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();

// Register Dashboard Service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Register Lookup Service
builder.Services.AddScoped<ILookupService, LookupService>();

// Week 2, Day 8: University & Department Services
builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Course Service
builder.Services.AddScoped<ICourseService, CourseService>();

// Level, Section, Group Services
builder.Services.AddScoped<ILevelService, LevelService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IGroupService, GroupService>();

// Term Service
builder.Services.AddScoped<ITermService, TermService>();

// Course Offering Service
builder.Services.AddScoped<ICourseOfferingService, CourseOfferingService>();

// Student Enrollment Service
builder.Services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();

// Timetable Service
builder.Services.AddScoped<ITimetableService, TimetableService>();

// Room Service
builder.Services.AddScoped<IRoomService, RoomService>();

// Quiz Repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
builder.Services.AddScoped<IQuizAssignmentRepository, QuizAssignmentRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();

// Quiz Service
builder.Services.AddScoped<IQuizService, QuizService>();

// Grading Services
builder.Services.AddScoped<IGradingService, GradingService>();
builder.Services.AddScoped<ITranscriptService, TranscriptService>();

// Report Services
builder.Services.AddScoped<IReportService, ReportService>();

// Face Recognition Services
builder.Services.Configure<FaceRecognitionOptions>(
    builder.Configuration.GetSection("FaceRecognition"));
builder.Services.AddFaceRecognition();
builder.Services.AddScoped<IFaceEnrollmentService, FaceEnrollmentService>();

// Mobile Attendance Services
builder.Services.Configure<AttendanceSettings>(
    builder.Configuration.GetSection("AttendanceSettings"));
builder.Services.AddScoped<IMobileAttendanceService, MobileAttendanceService>();

// Material Services
builder.Services.AddScoped<IMaterialService, MaterialService>();

// Daily Cleanup Services
builder.Services.AddScoped<IDailyCleanupService, DailyCleanupService>();
builder.Services.AddHostedService<DailyCleanupBackgroundService>();

builder.Services.AddControllersWithViews();

// Add API Controllers
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Smart ClassRoom API",
        Version = "v1",
        Description = "API for Smart ClassRoom Management System"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable Swagger (available in all environments)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart ClassRoom API v1");
    c.RoutePrefix = "swagger"; // Access at: /swagger
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Root path redirect based on authentication
app.MapGet("/", async (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect("/Admin/Dashboard");
    }
    else
    {
        context.Response.Redirect("/Account/Login");
    }
});

// Map MVC routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Map API Controllers
app.MapControllers();

app.Run();
