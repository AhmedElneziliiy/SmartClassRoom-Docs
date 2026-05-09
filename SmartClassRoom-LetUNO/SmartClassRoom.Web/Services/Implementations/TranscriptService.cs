using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.ViewModels.Grading;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class TranscriptService : ITranscriptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGradingService _gradingService;

    public TranscriptService(IUnitOfWork unitOfWork, IGradingService gradingService)
    {
        _unitOfWork = unitOfWork;
        _gradingService = gradingService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<TranscriptDto?> GenerateTranscriptDataAsync(int studentId)
    {
        var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
        if (student == null) return null;

        var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId);
        var completedEnrollments = enrollments
            .Where(e => e.Status == "Completed" && e.FinalGrade.HasValue)
            .OrderBy(e => e.CourseOffering.Term.StartDate)
            .ThenBy(e => e.CourseOffering.Course.Code)
            .ToList();

        var courseRecords = completedEnrollments.Select(e => new TranscriptCourseDto
        {
            Code = e.CourseOffering.Course.Code,
            CourseName = e.CourseOffering.Course.Name,
            Credits = e.CourseOffering.Course.Credits,
            LetterGrade = e.LetterGrade!,
            GradePoint = GetGradePoint(e.LetterGrade!),
            TermName = e.CourseOffering.Term.Name
        }).ToList();

        var gpaDto = await _gradingService.CalculateGPAAsync(studentId);

        return new TranscriptDto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            StudentCode = student.StudentCode,
            Email = student.Email,
            DepartmentName = student.Department?.Name,
            LevelName = student.Level?.Name,
            Courses = courseRecords,
            CumulativeGPA = gpaDto?.CumulativeGPA ?? 0,
            TotalCredits = gpaDto?.TotalCredits ?? 0,
            GeneratedDate = DateTime.UtcNow
        };
    }

    public async Task<byte[]> GenerateTranscriptPdfAsync(int studentId)
    {
        var data = await GenerateTranscriptDataAsync(studentId);
        if (data == null)
            throw new ArgumentException("Student not found or has no completed courses");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("Official Academic Transcript")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        // Student Information
                        col.Item().Element(StudentInfoSection);

                        col.Item().PaddingTop(0.5f, Unit.Centimetre);

                        // Course List
                        col.Item().Element(CourseListSection);

                        col.Item().PaddingTop(0.5f, Unit.Centimetre);

                        // GPA Summary
                        col.Item().Element(GPASummarySection);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span(data.GeneratedDate.ToString("MMMM dd, yyyy")).SemiBold();
                    });
            });
        }).GeneratePdf();

        void StudentInfoSection(IContainer container)
        {
            container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text($"Student Name: {data.StudentName}").SemiBold();
                column.Item().Text($"Student Code: {data.StudentCode}");
                column.Item().Text($"Email: {data.Email}");
                column.Item().Text($"Department: {data.DepartmentName ?? "N/A"}");
                column.Item().Text($"Level: {data.LevelName ?? "N/A"}");
            });
        }

        void CourseListSection(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Course Code
                    columns.RelativeColumn(3);   // Course Name
                    columns.ConstantColumn(60);  // Credits
                    columns.ConstantColumn(60);  // Grade
                    columns.ConstantColumn(100); // Term
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Code").SemiBold();
                    header.Cell().Element(CellStyle).Text("Course Name").SemiBold();
                    header.Cell().Element(CellStyle).Text("Credits").SemiBold();
                    header.Cell().Element(CellStyle).Text("Grade").SemiBold();
                    header.Cell().Element(CellStyle).Text("Term").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Background(Colors.Blue.Lighten3).Padding(5);
                    }
                });

                // Rows
                foreach (var course in data.Courses)
                {
                    table.Cell().Element(CellStyle).Text(course.Code);
                    table.Cell().Element(CellStyle).Text(course.CourseName);
                    table.Cell().Element(CellStyle).AlignCenter().Text(course.Credits.ToString());
                    table.Cell().Element(CellStyle).AlignCenter().Text(course.LetterGrade);
                    table.Cell().Element(CellStyle).Text(course.TermName);
                }

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                }
            });
        }

        void GPASummarySection(IContainer container)
        {
            container.Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
            {
                row.RelativeItem().Text($"Total Credits: {data.TotalCredits}").SemiBold();
                row.RelativeItem().AlignRight().Text($"Cumulative GPA: {data.CumulativeGPA:F2}").SemiBold().FontSize(14);
            });
        }
    }

    private static decimal GetGradePoint(string letterGrade)
    {
        return letterGrade switch
        {
            "A" => 4.0m,
            "B+" => 3.3m,
            "B" => 3.0m,
            "C+" => 2.3m,
            "C" => 2.0m,
            "D" => 1.0m,
            _ => 0.0m
        };
    }
}
