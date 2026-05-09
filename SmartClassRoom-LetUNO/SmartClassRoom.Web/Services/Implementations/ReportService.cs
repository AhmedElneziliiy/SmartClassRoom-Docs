using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Constants;
using SmartClassRoom.Web.Models.ViewModels.Reports;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGradingService _gradingService;

    public ReportService(IUnitOfWork unitOfWork, IGradingService gradingService)
    {
        _unitOfWork = unitOfWork;
        _gradingService = gradingService;
    }

    #region Attendance Reports

    public async Task<AttendanceReportDto> GenerateAttendanceReportDataAsync(
        int courseOfferingId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
            throw new ArgumentException("Course offering not found");

        var sessions = offering.Sessions
            .Where(s => (!dateFrom.HasValue || s.SessionDate >= dateFrom.Value) &&
                       (!dateTo.HasValue || s.SessionDate <= dateTo.Value))
            .OrderBy(s => s.SessionDate)
            .ToList();

        // If no sessions in date range, use term dates
        if (!sessions.Any())
        {
            // Use all sessions if available, otherwise use term dates as default
            sessions = offering.Sessions.OrderBy(s => s.SessionDate).ToList();

            if (!sessions.Any())
            {
                // No sessions at all - use term dates for the report header
                dateFrom = dateFrom ?? offering.Term.StartDate;
                dateTo = dateTo ?? offering.Term.EndDate;
            }
        }

        var enrollments = offering.Enrollments
            .Where(e => EnrollmentStatus.IsActive(e.Status))
            .ToList();

        var studentRecords = new List<AttendanceReportItemDto>();
        foreach (var enrollment in enrollments)
        {
            var stats = await _unitOfWork.Attendances
                .GetAttendanceStatisticsByCourseAsync(enrollment.StudentId, courseOfferingId);

            studentRecords.Add(new AttendanceReportItemDto
            {
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student.FullName,
                StudentCode = enrollment.Student.StudentCode ?? "N/A",
                TotalSessions = stats.TotalSessions,
                PresentCount = stats.PresentCount,
                AbsentCount = stats.AbsentCount,
                LateCount = stats.LateCount,
                ExcusedCount = stats.ExcusedCount,
                AttendancePercentage = stats.AttendancePercentage
            });
        }

        return new AttendanceReportDto
        {
            CourseOfferingId = courseOfferingId,
            CourseCode = offering.Course.Code,
            CourseName = offering.Course.Name,
            TermName = offering.Term.Name,
            TeacherName = offering.Teacher.FullName,
            DateFrom = dateFrom ?? (sessions.Any() ? sessions.First().SessionDate : offering.Term.StartDate),
            DateTo = dateTo ?? (sessions.Any() ? sessions.Last().SessionDate : offering.Term.EndDate),
            TotalSessions = sessions.Count,
            StudentRecords = studentRecords.OrderBy(s => s.StudentName).ToList(),
            GeneratedDate = DateTime.Now
        };
    }

    public async Task<byte[]> GenerateAttendanceReportPdfAsync(
        int courseOfferingId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var data = await GenerateAttendanceReportDataAsync(courseOfferingId, dateFrom, dateTo);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(column =>
                {
                    column.Item().Text("Attendance Report")
                        .FontSize(20).Bold().FontColor("#0d6efd");
                    column.Item().Text($"{data.CourseCode} - {data.CourseName}")
                        .FontSize(14);
                    column.Item().Text($"Term: {data.TermName} | Teacher: {data.TeacherName}")
                        .FontSize(10);
                    column.Item().Text($"Period: {data.DateFrom:MMM dd, yyyy} - {data.DateTo:MMM dd, yyyy}")
                        .FontSize(10);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(80);
                        columns.RelativeColumn(3);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(5).Text("#");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Code");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Student Name");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Total");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Present");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Absent");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Late");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Excused");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Attendance %");
                    });

                    int rowNum = 1;
                    foreach (var student in data.StudentRecords)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentCode);
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentName);
                        table.Cell().Background(bgColor).Padding(5).Text(student.TotalSessions.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.PresentCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.AbsentCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.LateCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.ExcusedCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text($"{student.AttendancePercentage:F1}%");

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateAttendanceReportExcelAsync(
        int courseOfferingId, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var data = await GenerateAttendanceReportDataAsync(courseOfferingId, dateFrom, dateTo);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Attendance Report");

        worksheet.Cells["A1:I1"].Merge = true;
        worksheet.Cells["A1"].Value = "Attendance Report";
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A2"].Value = $"{data.CourseCode} - {data.CourseName}";
        worksheet.Cells["A3"].Value = $"Term: {data.TermName} | Teacher: {data.TeacherName}";
        worksheet.Cells["A4"].Value = $"Period: {data.DateFrom:MMM dd, yyyy} - {data.DateTo:MMM dd, yyyy}";

        var headers = new[] { "#", "Code", "Student Name", "Total", "Present", "Absent", "Late", "Excused", "Attendance %" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[6, i + 1].Value = headers[i];
            worksheet.Cells[6, i + 1].Style.Font.Bold = true;
            worksheet.Cells[6, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[6, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 7;
        int num = 1;
        foreach (var student in data.StudentRecords)
        {
            worksheet.Cells[row, 1].Value = num++;
            worksheet.Cells[row, 2].Value = student.StudentCode;
            worksheet.Cells[row, 3].Value = student.StudentName;
            worksheet.Cells[row, 4].Value = student.TotalSessions;
            worksheet.Cells[row, 5].Value = student.PresentCount;
            worksheet.Cells[row, 6].Value = student.AbsentCount;
            worksheet.Cells[row, 7].Value = student.LateCount;
            worksheet.Cells[row, 8].Value = student.ExcusedCount;
            worksheet.Cells[row, 9].Value = $"{student.AttendancePercentage:F1}%";
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        worksheet.Cells[row + 1, 1].Value = $"Generated on: {data.GeneratedDate:MMM dd, yyyy HH:mm}";

        return package.GetAsByteArray();
    }

    #endregion

    #region Grade Reports

    public async Task<GradeReportDto> GenerateGradeReportDataAsync(int courseOfferingId)
    {
        var gradeSheet = await _gradingService.GetGradeSheetAsync(courseOfferingId);
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);

        if (offering == null)
            throw new ArgumentException("Course offering not found");

        var components = gradeSheet.Components.Select(c => new GradeComponentInfo
        {
            ComponentName = c.ComponentName,
            Weight = c.Weight,
            MaxScore = c.MaxScore
        }).ToList();

        var studentGrades = gradeSheet.Students.Select(s => new GradeReportItemDto
        {
            StudentId = s.StudentId,
            StudentName = s.StudentName,
            StudentCode = s.StudentCode,
            ComponentScores = s.ComponentScores.Zip(gradeSheet.Components, (score, comp) =>
                new { Name = comp.ComponentName, Score = score.Score })
                .ToDictionary(x => x.Name, x => x.Score),
            FinalGrade = s.FinalGrade,
            LetterGrade = s.LetterGrade
        }).ToList();

        var classAverage = studentGrades
            .Where(s => s.FinalGrade.HasValue)
            .DefaultIfEmpty()
            .Average(s => s?.FinalGrade ?? 0);

        return new GradeReportDto
        {
            CourseOfferingId = courseOfferingId,
            CourseCode = offering.Course.Code,
            CourseName = offering.Course.Name,
            TermName = offering.Term.Name,
            TeacherName = offering.Teacher.FullName,
            Components = components,
            StudentGrades = studentGrades.OrderBy(s => s.StudentName).ToList(),
            ClassAverage = classAverage,
            GeneratedDate = DateTime.Now
        };
    }

    public async Task<byte[]> GenerateGradeReportPdfAsync(int courseOfferingId)
    {
        var data = await GenerateGradeReportDataAsync(courseOfferingId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(column =>
                {
                    column.Item().Text("Grade Report")
                        .FontSize(20).Bold().FontColor("#198754");
                    column.Item().Text($"{data.CourseCode} - {data.CourseName}")
                        .FontSize(14);
                    column.Item().Text($"Term: {data.TermName} | Teacher: {data.TeacherName}")
                        .FontSize(10);
                    column.Item().Text($"Class Average: {data.ClassAverage:F2}%")
                        .FontSize(10).Bold();
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    var columnCount = 5 + data.Components.Count;
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(80);
                        columns.RelativeColumn(2);
                        foreach (var _ in data.Components)
                            columns.RelativeColumn(1);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(50);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(5).Text("#");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Code");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Student Name");
                        foreach (var comp in data.Components)
                        {
                            header.Cell().Background("#f8f9fa").Padding(5).Text($"{comp.ComponentName}\n({comp.Weight}%)");
                        }
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Final Grade");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Letter");
                    });

                    int rowNum = 1;
                    foreach (var student in data.StudentGrades)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentCode);
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentName);

                        foreach (var comp in data.Components)
                        {
                            var score = student.ComponentScores.GetValueOrDefault(comp.ComponentName);
                            table.Cell().Background(bgColor).Padding(5).Text(score.HasValue ? $"{score.Value:F1}" : "-");
                        }

                        table.Cell().Background(bgColor).Padding(5).Text(student.FinalGrade.HasValue ? $"{student.FinalGrade.Value:F2}%" : "-");
                        table.Cell().Background(bgColor).Padding(5).Text(student.LetterGrade ?? "-");

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateGradeReportExcelAsync(int courseOfferingId)
    {
        var data = await GenerateGradeReportDataAsync(courseOfferingId);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Grade Report");

        var lastCol = 5 + data.Components.Count;
        worksheet.Cells[1, 1, 1, lastCol].Merge = true;
        worksheet.Cells[1, 1].Value = "Grade Report";
        worksheet.Cells[1, 1].Style.Font.Size = 16;
        worksheet.Cells[1, 1].Style.Font.Bold = true;

        worksheet.Cells[2, 1].Value = $"{data.CourseCode} - {data.CourseName}";
        worksheet.Cells[3, 1].Value = $"Term: {data.TermName} | Teacher: {data.TeacherName}";
        worksheet.Cells[4, 1].Value = $"Class Average: {data.ClassAverage:F2}%";

        int col = 1;
        worksheet.Cells[6, col++].Value = "#";
        worksheet.Cells[6, col++].Value = "Code";
        worksheet.Cells[6, col++].Value = "Student Name";
        foreach (var comp in data.Components)
        {
            worksheet.Cells[6, col].Value = $"{comp.ComponentName} ({comp.Weight}%)";
            col++;
        }
        worksheet.Cells[6, col++].Value = "Final Grade";
        worksheet.Cells[6, col++].Value = "Letter";

        for (int i = 1; i <= lastCol; i++)
        {
            worksheet.Cells[6, i].Style.Font.Bold = true;
            worksheet.Cells[6, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[6, i].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 7;
        int num = 1;
        foreach (var student in data.StudentGrades)
        {
            col = 1;
            worksheet.Cells[row, col++].Value = num++;
            worksheet.Cells[row, col++].Value = student.StudentCode;
            worksheet.Cells[row, col++].Value = student.StudentName;

            foreach (var comp in data.Components)
            {
                var score = student.ComponentScores.GetValueOrDefault(comp.ComponentName);
                worksheet.Cells[row, col++].Value = score.HasValue ? (object)score.Value : "-";
            }

            worksheet.Cells[row, col++].Value = student.FinalGrade.HasValue ? $"{student.FinalGrade.Value:F2}%" : "-";
            worksheet.Cells[row, col++].Value = student.LetterGrade ?? "-";

            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        worksheet.Cells[row + 1, 1].Value = $"Generated on: {data.GeneratedDate:MMM dd, yyyy HH:mm}";

        return package.GetAsByteArray();
    }

    #endregion

    #region Enrollment Reports

    public async Task<EnrollmentReportDto> GenerateEnrollmentReportDataAsync(int? termId = null)
    {
        var offerings = termId.HasValue
            ? await _unitOfWork.CourseOfferings.GetByTermAsync(termId.Value)
            : await _unitOfWork.CourseOfferings.GetAllAsync();

        var offeringDetails = new List<EnrollmentReportItemDto>();
        int totalEnrollments = 0;

        foreach (var offering in offerings)
        {
            var offeringWithDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);
            if (offeringWithDetails == null) continue;

            var enrolledCount = offeringWithDetails.Enrollments.Count(e => e.Status == "Enrolled");
            var activeCount = offeringWithDetails.Enrollments.Count(e => EnrollmentStatus.IsActive(e.Status));
            var droppedCount = offeringWithDetails.Enrollments.Count(e => e.Status == "Dropped");
            var completedCount = offeringWithDetails.Enrollments.Count(e => e.Status == "Completed");

            totalEnrollments += enrolledCount;

            offeringDetails.Add(new EnrollmentReportItemDto
            {
                CourseCode = offeringWithDetails.Course.Code,
                CourseName = offeringWithDetails.Course.Name,
                TeacherName = offeringWithDetails.Teacher.FullName,
                MaxStudents = offeringWithDetails.MaxStudents,
                EnrolledCount = enrolledCount,
                ActiveCount = activeCount,
                DroppedCount = droppedCount,
                CompletedCount = completedCount,
                EnrollmentRate = offeringWithDetails.MaxStudents > 0
                    ? (decimal)enrolledCount / offeringWithDetails.MaxStudents * 100
                    : 0
            });
        }

        var term = termId.HasValue ? await _unitOfWork.Terms.GetByIdAsync(termId.Value) : null;

        return new EnrollmentReportDto
        {
            TermId = termId,
            TermName = term?.Name ?? "All Terms",
            TotalOfferings = offerings.Count(),
            TotalEnrollments = totalEnrollments,
            Offerings = offeringDetails.OrderBy(o => o.CourseCode).ToList(),
            GeneratedDate = DateTime.Now
        };
    }

    public async Task<byte[]> GenerateEnrollmentReportPdfAsync(int? termId = null)
    {
        var data = await GenerateEnrollmentReportDataAsync(termId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(column =>
                {
                    column.Item().Text("Enrollment Report")
                        .FontSize(20).Bold().FontColor("#0dcaf0");
                    column.Item().Text($"Term: {data.TermName}")
                        .FontSize(14);
                    column.Item().Text($"Total Offerings: {data.TotalOfferings} | Total Enrollments: {data.TotalEnrollments}")
                        .FontSize(10);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(80);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(70);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(5).Text("#");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Code");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Course Name");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Teacher");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Max");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Enrolled");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Active");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Dropped");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Completed");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Rate %");
                    });

                    int rowNum = 1;
                    foreach (var offering in data.Offerings)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(offering.CourseCode);
                        table.Cell().Background(bgColor).Padding(5).Text(offering.CourseName);
                        table.Cell().Background(bgColor).Padding(5).Text(offering.TeacherName);
                        table.Cell().Background(bgColor).Padding(5).Text(offering.MaxStudents.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(offering.EnrolledCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(offering.ActiveCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(offering.DroppedCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(offering.CompletedCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text($"{offering.EnrollmentRate:F1}%");

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateEnrollmentReportExcelAsync(int? termId = null)
    {
        var data = await GenerateEnrollmentReportDataAsync(termId);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Enrollment Report");

        worksheet.Cells["A1:J1"].Merge = true;
        worksheet.Cells["A1"].Value = "Enrollment Report";
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A2"].Value = $"Term: {data.TermName}";
        worksheet.Cells["A3"].Value = $"Total Offerings: {data.TotalOfferings} | Total Enrollments: {data.TotalEnrollments}";

        var headers = new[] { "#", "Code", "Course Name", "Teacher", "Max", "Enrolled", "Active", "Dropped", "Completed", "Rate %" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[5, i + 1].Value = headers[i];
            worksheet.Cells[5, i + 1].Style.Font.Bold = true;
            worksheet.Cells[5, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 6;
        int num = 1;
        foreach (var offering in data.Offerings)
        {
            worksheet.Cells[row, 1].Value = num++;
            worksheet.Cells[row, 2].Value = offering.CourseCode;
            worksheet.Cells[row, 3].Value = offering.CourseName;
            worksheet.Cells[row, 4].Value = offering.TeacherName;
            worksheet.Cells[row, 5].Value = offering.MaxStudents;
            worksheet.Cells[row, 6].Value = offering.EnrolledCount;
            worksheet.Cells[row, 7].Value = offering.ActiveCount;
            worksheet.Cells[row, 8].Value = offering.DroppedCount;
            worksheet.Cells[row, 9].Value = offering.CompletedCount;
            worksheet.Cells[row, 10].Value = $"{offering.EnrollmentRate:F1}%";
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        worksheet.Cells[row + 1, 1].Value = $"Generated on: {data.GeneratedDate:MMM dd, yyyy HH:mm}";

        return package.GetAsByteArray();
    }

    #endregion

    #region List Available Reports

    public async Task<List<ReportListItemDto>> GetAvailableReportsAsync(
        int? termId = null, string? reportType = null)
    {
        var offerings = termId.HasValue
            ? await _unitOfWork.CourseOfferings.GetByTermAsync(termId.Value)
            : await _unitOfWork.CourseOfferings.GetAllAsync();

        var reports = new List<ReportListItemDto>();

        foreach (var offering in offerings)
        {
            var offeringWithDetails = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offering.Id);
            if (offeringWithDetails == null) continue;

            var enrolledCount = offeringWithDetails.Enrollments.Count(e => EnrollmentStatus.IsActive(e.Status));

            if (reportType == null || reportType == "Attendance")
            {
                reports.Add(new ReportListItemDto
                {
                    Id = offering.Id,
                    Title = $"{offeringWithDetails.Course.Code} Attendance Report",
                    Type = "Attendance",
                    CourseCode = offeringWithDetails.Course.Code,
                    TermName = offeringWithDetails.Term.Name,
                    DateFrom = offeringWithDetails.Term.StartDate,
                    DateTo = offeringWithDetails.Term.EndDate,
                    RecordCount = enrolledCount
                });
            }

            if (reportType == null || reportType == "Grade")
            {
                var hasComponents = offeringWithDetails.GradeComponents?.Any() ?? false;
                if (hasComponents)
                {
                    reports.Add(new ReportListItemDto
                    {
                        Id = offering.Id,
                        Title = $"{offeringWithDetails.Course.Code} Grade Report",
                        Type = "Grade",
                        CourseCode = offeringWithDetails.Course.Code,
                        TermName = offeringWithDetails.Term.Name,
                        RecordCount = enrolledCount
                    });
                }
            }
        }

        return reports.OrderByDescending(r => r.TermName).ThenBy(r => r.CourseCode).ToList();
    }

    #endregion

    #region Student Reports

    public async Task<StudentReportDto> GenerateStudentReportDataAsync(int studentId)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null)
            throw new ArgumentException("Student not found");

        var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId);
        var activeEnrollments = enrollments.Where(e => EnrollmentStatus.IsActive(e.Status)).ToList();

        var currentCourses = new List<StudentCourseInfo>();
        string currentTermName = "N/A";

        foreach (var enrollment in activeEnrollments)
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);
            if (offering == null) continue;

            currentTermName = offering.Term.Name;

            var attendanceStats = await _unitOfWork.Attendances
                .GetAttendanceStatisticsByCourseAsync(studentId, offering.Id);

            var grades = await _unitOfWork.Grades.GetGradesByStudentAndOfferingAsync(studentId, offering.Id);
            decimal? finalGrade = null;
            string? letterGrade = null;

            if (grades.Any())
            {
                var gradeSheet = await _gradingService.GetGradeSheetAsync(offering.Id);
                var studentGrade = gradeSheet.Students.FirstOrDefault(s => s.StudentId == studentId);
                finalGrade = studentGrade?.FinalGrade;
                letterGrade = studentGrade?.LetterGrade;
            }

            currentCourses.Add(new StudentCourseInfo
            {
                CourseCode = offering.Course.Code,
                CourseName = offering.Course.Name,
                TermName = offering.Term.Name,
                TeacherName = offering.Teacher.FullName,
                Status = enrollment.Status,
                FinalGrade = finalGrade,
                LetterGrade = letterGrade,
                Credits = offering.Course.Credits,
                AttendancePercentage = attendanceStats.AttendancePercentage
            });
        }

        var gpaResult = await _gradingService.CalculateGPAAsync(studentId);

        return new StudentReportDto
        {
            StudentId = studentId,
            StudentCode = student.StudentCode ?? "N/A",
            StudentName = student.FullName,
            Email = student.Email ?? "N/A",
            PhoneNumber = student.PhoneNumber,
            Level = student.Level?.Name,
            Section = student.Section?.Name,
            Department = student.Department?.Name,
            CurrentCourses = currentCourses.OrderBy(c => c.CourseCode).ToList(),
            CurrentTermGPA = gpaResult.TermGPA,
            CumulativeGPA = gpaResult.CumulativeGPA,
            TotalCreditsCompleted = gpaResult.TotalCredits,
            CurrentTerm = currentTermName,
            GeneratedDate = DateTime.Now
        };
    }

    public async Task<byte[]> GenerateStudentReportPdfAsync(int studentId)
    {
        var data = await GenerateStudentReportDataAsync(studentId);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(column =>
                {
                    column.Item().Text("Student Academic Report")
                        .FontSize(20).Bold().FontColor("#0d6efd");
                    column.Item().PaddingTop(5).Text($"Generated on: {data.GeneratedDate:MMMM dd, yyyy}")
                        .FontSize(10).FontColor("#6c757d");
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Item().Text("Student Information").FontSize(16).Bold();
                    column.Item().PaddingVertical(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Padding(5).Text("Student Code:").Bold();
                        table.Cell().Padding(5).Text(data.StudentCode);

                        table.Cell().Padding(5).Text("Full Name:").Bold();
                        table.Cell().Padding(5).Text(data.StudentName);

                        table.Cell().Padding(5).Text("Email:").Bold();
                        table.Cell().Padding(5).Text(data.Email);

                        table.Cell().Padding(5).Text("Phone:").Bold();
                        table.Cell().Padding(5).Text(data.PhoneNumber ?? "N/A");

                        table.Cell().Padding(5).Text("Level:").Bold();
                        table.Cell().Padding(5).Text(data.Level ?? "N/A");

                        table.Cell().Padding(5).Text("Section:").Bold();
                        table.Cell().Padding(5).Text(data.Section ?? "N/A");

                        table.Cell().Padding(5).Text("Department:").Bold();
                        table.Cell().Padding(5).Text(data.Department ?? "N/A");

                        table.Cell().Padding(5).Text("Current Term:").Bold();
                        table.Cell().Padding(5).Text(data.CurrentTerm);
                    });

                    column.Item().PaddingTop(20).Text("Academic Performance").FontSize(16).Bold();
                    column.Item().PaddingVertical(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Cell().Padding(5).Text("Current Term GPA:").Bold();
                        table.Cell().Padding(5).Text(data.CurrentTermGPA.HasValue ? $"{data.CurrentTermGPA.Value:F2}" : "N/A");

                        table.Cell().Padding(5).Text("Cumulative GPA:").Bold();
                        table.Cell().Padding(5).Text(data.CumulativeGPA.HasValue ? $"{data.CumulativeGPA.Value:F2}" : "N/A");

                        table.Cell().Padding(5).Text("Total Credits:").Bold();
                        table.Cell().Padding(5).Text(data.TotalCreditsCompleted.ToString());
                    });

                    if (data.CurrentCourses.Any())
                    {
                        column.Item().PaddingTop(20).Text("Current Courses").FontSize(16).Bold();
                        column.Item().PaddingVertical(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Code").Bold();
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Course").Bold();
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Teacher").Bold();
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Credits").Bold();
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Grade").Bold();
                                header.Cell().Background("#f8f9fa").Padding(5).Text("Attendance").Bold();
                            });

                            foreach (var course in data.CurrentCourses)
                            {
                                table.Cell().Padding(5).Text(course.CourseCode);
                                table.Cell().Padding(5).Text(course.CourseName);
                                table.Cell().Padding(5).Text(course.TeacherName);
                                table.Cell().Padding(5).Text(course.Credits.ToString());
                                table.Cell().Padding(5).Text(course.LetterGrade ?? "-");
                                table.Cell().Padding(5).Text(course.AttendancePercentage.HasValue ? $"{course.AttendancePercentage.Value:F1}%" : "-");
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<StudentListReportDto> GenerateStudentListReportDataAsync(
        int? termId = null, string? level = null, string? department = null)
    {
        var allStudents = await _unitOfWork.Students.GetAllAsync();
        var students = allStudents.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(level))
            students = students.Where(s => s.Level?.Name == level);

        if (!string.IsNullOrWhiteSpace(department))
            students = students.Where(s => s.Department?.Name == department);

        var studentSummaries = new List<StudentSummaryDto>();

        foreach (var student in students)
        {
            var enrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(student.Id);
            var currentEnrollments = enrollments
                .Where(e => EnrollmentStatus.IsActive(e.Status))
                .ToList();

            if (termId.HasValue)
            {
                var termEnrollments = new List<SmartClassRoom.Web.Models.Entities.Academic.StudentEnrollment>();
                foreach (var enrollment in currentEnrollments)
                {
                    var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(enrollment.CourseOfferingId);
                    if (offering?.TermId == termId.Value)
                        termEnrollments.Add(enrollment);
                }
                currentEnrollments = termEnrollments;
            }

            if (termId.HasValue && !currentEnrollments.Any())
                continue;

            var gpa = await _gradingService.CalculateGPAAsync(student.Id);

            studentSummaries.Add(new StudentSummaryDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentCode ?? "N/A",
                StudentName = student.FullName,
                Email = student.Email ?? "N/A",
                Level = student.Level?.Name,
                Section = student.Section?.Name,
                Department = student.Department?.Name,
                CurrentCourses = currentEnrollments.Count,
                CurrentGPA = gpa.TermGPA,
                CumulativeGPA = gpa.CumulativeGPA,
                Status = student.IsActive ? "Active" : "Inactive"
            });
        }

        var term = termId.HasValue ? await _unitOfWork.Terms.GetByIdAsync(termId.Value) : null;

        return new StudentListReportDto
        {
            FilterTerm = term?.Name,
            FilterLevel = level,
            FilterDepartment = department,
            Students = studentSummaries.OrderBy(s => s.StudentName).ToList(),
            TotalStudents = studentSummaries.Count,
            GeneratedDate = DateTime.Now
        };
    }

    public async Task<byte[]> GenerateStudentListReportPdfAsync(
        int? termId = null, string? level = null, string? department = null)
    {
        var data = await GenerateStudentListReportDataAsync(termId, level, department);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(column =>
                {
                    column.Item().Text("Student List Report")
                        .FontSize(20).Bold().FontColor("#0d6efd");

                    var filters = new List<string>();
                    if (!string.IsNullOrEmpty(data.FilterTerm))
                        filters.Add($"Term: {data.FilterTerm}");
                    if (!string.IsNullOrEmpty(data.FilterLevel))
                        filters.Add($"Level: {data.FilterLevel}");
                    if (!string.IsNullOrEmpty(data.FilterDepartment))
                        filters.Add($"Department: {data.FilterDepartment}");

                    if (filters.Any())
                        column.Item().Text(string.Join(" | ", filters)).FontSize(10);

                    column.Item().Text($"Total Students: {data.TotalStudents}").FontSize(12).Bold();
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.ConstantColumn(70);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(60);
                        columns.RelativeColumn(1);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(60);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(5).Text("#");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Code");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Name");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Email");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Level");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Department");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Courses");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Current GPA");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("CGPA");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Status");
                    });

                    int rowNum = 1;
                    foreach (var student in data.Students)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentCode);
                        table.Cell().Background(bgColor).Padding(5).Text(student.StudentName);
                        table.Cell().Background(bgColor).Padding(5).Text(student.Email);
                        table.Cell().Background(bgColor).Padding(5).Text(student.Level ?? "-");
                        table.Cell().Background(bgColor).Padding(5).Text(student.Department ?? "-");
                        table.Cell().Background(bgColor).Padding(5).Text(student.CurrentCourses.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(student.CurrentGPA.HasValue ? $"{student.CurrentGPA.Value:F2}" : "-");
                        table.Cell().Background(bgColor).Padding(5).Text(student.CumulativeGPA.HasValue ? $"{student.CumulativeGPA.Value:F2}" : "-");
                        table.Cell().Background(bgColor).Padding(5).Text(student.Status);

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    #endregion

    #region Student Multi-Course Attendance Report

    public async Task<StudentMultiCourseAttendanceDto> GenerateStudentMultiCourseAttendanceDataAsync(
        int studentId, List<int> courseOfferingIds)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null)
            throw new ArgumentException("Student not found");

        var result = new StudentMultiCourseAttendanceDto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            StudentCode = student.StudentCode ?? "",
            GeneratedDate = DateTime.UtcNow,
            Courses = new List<CourseAttendanceSummary>()
        };

        foreach (var offeringId in courseOfferingIds)
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(offeringId);
            if (offering == null) continue;

            // Get all sessions for this course
            var sessions = offering.Sessions.ToList();
            var sessionIds = sessions.Select(s => s.Id).ToList();

            // Get student's attendance records for these sessions
            var attendances = await _unitOfWork.Attendances.GetByStudentAndCourseOfferingAsync(studentId, offeringId);

            var presentCount = attendances.Count(a => a.Status == "Present");
            var absentCount = sessions.Count() - attendances.Count() + attendances.Count(a => a.Status == "Absent");
            var lateCount = attendances.Count(a => a.Status == "Late");
            var excusedCount = attendances.Count(a => a.Status == "Excused");

            var attendancePercentage = sessions.Count() > 0
                ? (decimal)(presentCount + lateCount + excusedCount) / sessions.Count() * 100
                : 0;

            result.Courses.Add(new CourseAttendanceSummary
            {
                CourseOfferingId = offeringId,
                CourseName = offering.Course?.Name ?? "",
                CourseCode = offering.Course?.Code ?? "",
                TermName = offering.Term?.Name ?? "",
                TotalSessions = sessions.Count,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                LateCount = lateCount,
                ExcusedCount = excusedCount,
                AttendancePercentage = attendancePercentage
            });
        }

        return result;
    }

    public async Task<byte[]> GenerateStudentMultiCourseAttendancePdfAsync(
        int studentId, List<int> courseOfferingIds)
    {
        var data = await GenerateStudentMultiCourseAttendanceDataAsync(studentId, courseOfferingIds);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Student Multi-Course Attendance Report")
                        .FontSize(18).Bold().FontColor("#6f42c1");
                    column.Item().PaddingTop(10).AlignCenter().Text($"Student: {data.StudentName} ({data.StudentCode})")
                        .FontSize(12);
                });

                page.Content().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(5).Text("#");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Course");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Code");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Term");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Sessions");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Present");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Absent");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Late");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Excused");
                        header.Cell().Background("#f8f9fa").Padding(5).Text("Attendance %");
                    });

                    int rowNum = 1;
                    foreach (var course in data.Courses)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(course.CourseName);
                        table.Cell().Background(bgColor).Padding(5).Text(course.CourseCode);
                        table.Cell().Background(bgColor).Padding(5).Text(course.TermName);
                        table.Cell().Background(bgColor).Padding(5).Text(course.TotalSessions.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(course.PresentCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(course.AbsentCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(course.LateCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(course.ExcusedCount.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text($"{course.AttendancePercentage:F1}%");

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateStudentMultiCourseAttendanceExcelAsync(
        int studentId, List<int> courseOfferingIds)
    {
        var data = await GenerateStudentMultiCourseAttendanceDataAsync(studentId, courseOfferingIds);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Student Attendance");

        // Title
        worksheet.Cells["A1:J1"].Merge = true;
        worksheet.Cells["A1"].Value = "Student Multi-Course Attendance Report";
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Student info
        worksheet.Cells["A2:J2"].Merge = true;
        worksheet.Cells["A2"].Value = $"Student: {data.StudentName} ({data.StudentCode})";
        worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Headers
        var headers = new[] { "#", "Course", "Code", "Term", "Sessions", "Present", "Absent", "Late", "Excused", "Attendance %" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[4, i + 1].Value = headers[i];
            worksheet.Cells[4, i + 1].Style.Font.Bold = true;
            worksheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 5;
        int num = 1;
        foreach (var course in data.Courses)
        {
            worksheet.Cells[row, 1].Value = num++;
            worksheet.Cells[row, 2].Value = course.CourseName;
            worksheet.Cells[row, 3].Value = course.CourseCode;
            worksheet.Cells[row, 4].Value = course.TermName;
            worksheet.Cells[row, 5].Value = course.TotalSessions;
            worksheet.Cells[row, 6].Value = course.PresentCount;
            worksheet.Cells[row, 7].Value = course.AbsentCount;
            worksheet.Cells[row, 8].Value = course.LateCount;
            worksheet.Cells[row, 9].Value = course.ExcusedCount;
            worksheet.Cells[row, 10].Value = $"{course.AttendancePercentage:F1}%";
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    #endregion

    #region Course Student Attendance Report (Session-by-Session)

    public async Task<CourseStudentAttendanceDto> GenerateCourseStudentAttendanceDataAsync(
        int courseOfferingId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
            throw new ArgumentException("Course offering not found");

        var result = new CourseStudentAttendanceDto
        {
            CourseOfferingId = courseOfferingId,
            CourseName = offering.Course?.Name ?? "",
            CourseCode = offering.Course?.Code ?? "",
            TermName = offering.Term?.Name ?? "",
            TeacherName = offering.Teacher?.FullName ?? "",
            GeneratedDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Sessions = new List<SessionDetailDto>(),
            Students = new List<StudentAttendanceDetailDto>()
        };

        // Get sessions within date range
        var sessions = offering.Sessions
            .Where(s => (!startDate.HasValue || s.SessionDate >= startDate.Value) &&
                       (!endDate.HasValue || s.SessionDate <= endDate.Value))
            .OrderBy(s => s.SessionDate)
            .ToList();

        // Build session list
        foreach (var session in sessions)
        {
            result.Sessions.Add(new SessionDetailDto
            {
                SessionId = session.Id,
                SessionDate = session.SessionDate,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status ?? ""
            });
        }

        // Get all enrolled students
        var enrollments = await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentsAsync(courseOfferingId);
        var enrolledStudents = enrollments.Where(e => e.Status == "Enrolled").ToList();

        foreach (var enrollment in enrolledStudents)
        {
            var student = enrollment.Student;
            if (student == null) continue;

            // Get all attendance records for this student in these sessions
            var attendances = await _unitOfWork.Attendances.GetByStudentAndCourseOfferingAsync(student.Id, courseOfferingId);
            var sessionAttendances = attendances.Where(a => sessions.Any(s => s.Id == a.SessionId)).ToList();

            var presentCount = sessionAttendances.Count(a => a.Status == "Present");
            var lateCount = sessionAttendances.Count(a => a.Status == "Late");
            var excusedCount = sessionAttendances.Count(a => a.Status == "Excused");
            var absentCount = sessions.Count - sessionAttendances.Count;

            var attendancePercentage = sessions.Count > 0
                ? (decimal)(presentCount + lateCount + excusedCount) / sessions.Count * 100
                : 0;

            // Calculate late arrivals
            var lateArrivals = new List<int>();
            foreach (var attendance in sessionAttendances.Where(a => a.Status == "Late"))
            {
                var session = sessions.FirstOrDefault(s => s.Id == attendance.SessionId);
                if (session != null)
                {
                    var sessionStart = session.SessionDate.Date.Add(session.StartTime);
                    var minutesLate = (int)(attendance.CheckInTime - sessionStart).TotalMinutes;
                    if (minutesLate > 0)
                        lateArrivals.Add(minutesLate);
                }
            }

            var avgLateMinutes = lateArrivals.Any() ? TimeSpan.FromMinutes(lateArrivals.Average()) : (TimeSpan?)null;

            // Build session-by-session attendance list
            var sessionAttendanceList = new List<SessionAttendanceStatus>();
            foreach (var session in sessions)
            {
                var attendance = sessionAttendances.FirstOrDefault(a => a.SessionId == session.Id);
                var sessionStart = session.SessionDate.Date.Add(session.StartTime);

                int? minutesLate = null;
                if (attendance != null && attendance.Status == "Late")
                {
                    minutesLate = (int)(attendance.CheckInTime - sessionStart).TotalMinutes;
                    if (minutesLate < 0) minutesLate = null;
                }

                sessionAttendanceList.Add(new SessionAttendanceStatus
                {
                    SessionId = session.Id,
                    SessionDate = session.SessionDate,
                    Status = attendance?.Status ?? "Absent",
                    CheckInTime = attendance?.CheckInTime,
                    CheckOutTime = attendance?.CheckOutTime,
                    MinutesLate = minutesLate
                });
            }

            result.Students.Add(new StudentAttendanceDetailDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                StudentCode = student.StudentCode ?? "",
                TotalSessions = sessions.Count,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                LateCount = lateCount,
                ExcusedCount = excusedCount,
                AttendancePercentage = attendancePercentage,
                LateArrivals = lateArrivals.Count,
                AverageLateMinutes = avgLateMinutes,
                SessionAttendances = sessionAttendanceList
            });
        }

        return result;
    }

    public async Task<byte[]> GenerateCourseStudentAttendancePdfAsync(
        int courseOfferingId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var data = await GenerateCourseStudentAttendanceDataAsync(courseOfferingId, startDate, endDate);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Course Student Attendance Report")
                        .FontSize(16).Bold().FontColor("#0d6efd");
                    column.Item().PaddingTop(5).AlignCenter().Text($"{data.CourseName} ({data.CourseCode})")
                        .FontSize(11);
                    column.Item().AlignCenter().Text($"Term: {data.TermName} | Teacher: {data.TeacherName}")
                        .FontSize(9);
                    if (data.StartDate.HasValue || data.EndDate.HasValue)
                    {
                        var dateRange = $"Period: {data.StartDate?.ToString("MMM dd, yyyy") ?? "Start"} - {data.EndDate?.ToString("MMM dd, yyyy") ?? "End"}";
                        column.Item().AlignCenter().Text(dateRange).FontSize(9);
                    }
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    // Dynamic columns based on number of sessions (limit to first 10 for PDF)
                    var maxSessions = Math.Min(data.Sessions.Count, 10);
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // #
                        columns.RelativeColumn(2); // Name
                        columns.ConstantColumn(45); // Code
                        columns.ConstantColumn(35); // Total
                        columns.ConstantColumn(35); // Present
                        columns.ConstantColumn(35); // Absent
                        columns.ConstantColumn(35); // Late
                        columns.ConstantColumn(40); // %
                        columns.ConstantColumn(50); // Late Stats
                        for (int i = 0; i < maxSessions; i++)
                            columns.ConstantColumn(30); // Session columns
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#f8f9fa").Padding(3).Text("#").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Student").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Code").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Total").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Pres").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Abs").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Late").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("%").FontSize(8);
                        header.Cell().Background("#f8f9fa").Padding(3).Text("Avg Late").FontSize(8);
                        for (int i = 0; i < maxSessions; i++)
                        {
                            var sessionDate = data.Sessions[i].SessionDate.ToString("MM/dd");
                            header.Cell().Background("#f8f9fa").Padding(3).Text(sessionDate).FontSize(7);
                        }
                    });

                    int rowNum = 1;
                    foreach (var student in data.Students)
                    {
                        var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                        table.Cell().Background(bgColor).Padding(3).Text(rowNum.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.StudentName).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.StudentCode).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.TotalSessions.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.PresentCount.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.AbsentCount.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.LateCount.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text($"{student.AttendancePercentage:F0}%").FontSize(8);
                        table.Cell().Background(bgColor).Padding(3).Text(student.AverageLateMinutes.HasValue ? $"{student.AverageLateMinutes.Value.TotalMinutes:F0}m" : "-").FontSize(8);

                        for (int i = 0; i < maxSessions; i++)
                        {
                            var sessionId = data.Sessions[i].SessionId;
                            var attendance = student.SessionAttendances.FirstOrDefault(a => a.SessionId == sessionId);
                            var symbol = attendance?.Status switch
                            {
                                "Present" => "✓",
                                "Late" => "L",
                                "Excused" => "E",
                                _ => "✗"
                            };
                            table.Cell().Background(bgColor).Padding(3).AlignCenter().Text(symbol).FontSize(8);
                        }

                        rowNum++;
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(TextStyle.Default.FontSize(7));
                    text.Span("Generated: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                    text.Span(" | ✓=Present, L=Late, E=Excused, ✗=Absent | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateCourseStudentAttendanceExcelAsync(
        int courseOfferingId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var data = await GenerateCourseStudentAttendanceDataAsync(courseOfferingId, startDate, endDate);

        using var package = new ExcelPackage();

        // Sheet 1: Summary
        var summarySheet = package.Workbook.Worksheets.Add("Summary");
        summarySheet.Cells["A1"].Value = "Course Student Attendance Report";
        summarySheet.Cells["A1"].Style.Font.Size = 14;
        summarySheet.Cells["A1"].Style.Font.Bold = true;
        summarySheet.Cells["A2"].Value = $"{data.CourseName} ({data.CourseCode}) - {data.TermName}";

        var headers = new[] { "#", "Code", "Student Name", "Total", "Present", "Absent", "Late", "Excused", "Attendance %", "Late Arrivals", "Avg Late (min)" };
        for (int i = 0; i < headers.Length; i++)
        {
            summarySheet.Cells[4, i + 1].Value = headers[i];
            summarySheet.Cells[4, i + 1].Style.Font.Bold = true;
            summarySheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            summarySheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        int row = 5;
        int num = 1;
        foreach (var student in data.Students)
        {
            summarySheet.Cells[row, 1].Value = num++;
            summarySheet.Cells[row, 2].Value = student.StudentCode;
            summarySheet.Cells[row, 3].Value = student.StudentName;
            summarySheet.Cells[row, 4].Value = student.TotalSessions;
            summarySheet.Cells[row, 5].Value = student.PresentCount;
            summarySheet.Cells[row, 6].Value = student.AbsentCount;
            summarySheet.Cells[row, 7].Value = student.LateCount;
            summarySheet.Cells[row, 8].Value = student.ExcusedCount;
            summarySheet.Cells[row, 9].Value = $"{student.AttendancePercentage:F1}%";
            summarySheet.Cells[row, 10].Value = student.LateArrivals;
            summarySheet.Cells[row, 11].Value = student.AverageLateMinutes.HasValue ? $"{student.AverageLateMinutes.Value.TotalMinutes:F0}" : "-";
            row++;
        }
        summarySheet.Cells[summarySheet.Dimension.Address].AutoFitColumns();

        // Sheet 2: Session-by-Session Matrix
        var matrixSheet = package.Workbook.Worksheets.Add("Session Matrix");
        matrixSheet.Cells["A1"].Value = "Session-by-Session Attendance";
        matrixSheet.Cells["A1"].Style.Font.Bold = true;

        matrixSheet.Cells[3, 1].Value = "Student Code";
        matrixSheet.Cells[3, 2].Value = "Student Name";
        int col = 3;
        foreach (var session in data.Sessions)
        {
            matrixSheet.Cells[3, col].Value = session.SessionDate.ToString("MM/dd");
            matrixSheet.Cells[3, col].Style.Font.Bold = true;
            col++;
        }

        row = 4;
        foreach (var student in data.Students)
        {
            matrixSheet.Cells[row, 1].Value = student.StudentCode;
            matrixSheet.Cells[row, 2].Value = student.StudentName;
            col = 3;
            foreach (var session in data.Sessions)
            {
                var attendance = student.SessionAttendances.FirstOrDefault(a => a.SessionId == session.SessionId);
                matrixSheet.Cells[row, col].Value = attendance?.Status ?? "Absent";
                col++;
            }
            row++;
        }
        matrixSheet.Cells[matrixSheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    #endregion

    #region Teacher Attendance Report

    public async Task<TeacherAttendanceDto> GenerateTeacherAttendanceDataAsync(
        int teacherId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
        if (teacher == null)
            throw new ArgumentException("Teacher not found");

        var department = teacher.DepartmentId.HasValue
            ? await _unitOfWork.Departments.GetByIdAsync(teacher.DepartmentId.Value)
            : null;

        var result = new TeacherAttendanceDto
        {
            TeacherId = teacherId,
            TeacherName = teacher.FullName,
            DepartmentName = department?.Name ?? "",
            GeneratedDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Sessions = new List<TeacherSessionDetailDto>()
        };

        // Get all course offerings taught by this teacher
        var offerings = await _unitOfWork.CourseOfferings.GetByTeacherAsync(teacherId);

        var allSessions = new List<Models.Entities.Scheduling.Session>();
        foreach (var offering in offerings)
        {
            var sessions = offering.Sessions
                .Where(s => (!startDate.HasValue || s.SessionDate >= startDate.Value) &&
                           (!endDate.HasValue || s.SessionDate <= endDate.Value))
                .ToList();
            allSessions.AddRange(sessions);
        }

        allSessions = allSessions.OrderBy(s => s.SessionDate).ToList();

        // Get teacher attendance records
        var teacherAttendances = startDate.HasValue && endDate.HasValue
            ? await _unitOfWork.TeacherAttendances.GetByTeacherAndDateRangeAsync(teacherId, startDate.Value, endDate.Value)
            : await _unitOfWork.TeacherAttendances.GetByTeacherAsync(teacherId);
        var teacherAttendanceList = teacherAttendances.ToList();

        int attended = 0;
        int lateCount = 0;

        foreach (var session in allSessions)
        {
            var teacherAttendance = teacherAttendanceList.FirstOrDefault(ta => ta.SessionId == session.Id);
            var sessionStart = session.SessionDate.Date.Add(session.StartTime);

            string status = "Absent";
            DateTime? checkIn = null;
            DateTime? checkOut = null;
            int? minutesLate = null;

            if (teacherAttendance != null)
            {
                attended++;
                checkIn = teacherAttendance.CheckInTime;
                checkOut = teacherAttendance.CheckOutTime;

                // Check if late
                if (teacherAttendance.CheckInTime > sessionStart)
                {
                    minutesLate = (int)(teacherAttendance.CheckInTime - sessionStart).TotalMinutes;
                    if (minutesLate > 0)
                    {
                        status = "Late";
                        lateCount++;
                    }
                    else
                    {
                        status = "Present";
                        minutesLate = null;
                    }
                }
                else
                {
                    status = "Present";
                }
            }

            var courseOffering = offerings.FirstOrDefault(o => o.Id == session.CourseOfferingId);
            var room = session.RoomId.HasValue
                ? await _unitOfWork.Rooms.GetByIdAsync(session.RoomId.Value)
                : null;

            result.Sessions.Add(new TeacherSessionDetailDto
            {
                SessionId = session.Id,
                SessionDate = session.SessionDate,
                ScheduledStartTime = session.StartTime,
                ScheduledEndTime = session.EndTime,
                CourseName = courseOffering?.Course?.Name ?? "",
                CourseCode = courseOffering?.Course?.Code ?? "",
                Status = status,
                ActualCheckInTime = checkIn,
                ActualCheckOutTime = checkOut,
                MinutesLate = minutesLate,
                RoomName = room?.Name ?? ""
            });
        }

        result.TotalScheduledSessions = allSessions.Count;
        result.SessionsAttended = attended;
        result.SessionsMissed = allSessions.Count - attended;
        result.LateArrivals = lateCount;
        result.AttendancePercentage = allSessions.Count > 0
            ? (decimal)attended / allSessions.Count * 100
            : 0;

        return result;
    }

    public async Task<byte[]> GenerateTeacherAttendancePdfAsync(
        int teacherId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var data = await GenerateTeacherAttendanceDataAsync(teacherId, startDate, endDate);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Teacher Attendance Report")
                        .FontSize(18).Bold().FontColor("#fd7e14");
                    column.Item().PaddingTop(10).AlignCenter().Text($"Teacher: {data.TeacherName}")
                        .FontSize(12);
                    column.Item().AlignCenter().Text($"Department: {data.DepartmentName}")
                        .FontSize(10);
                    if (data.StartDate.HasValue || data.EndDate.HasValue)
                    {
                        var dateRange = $"Period: {data.StartDate?.ToString("MMM dd, yyyy") ?? "Start"} - {data.EndDate?.ToString("MMM dd, yyyy") ?? "End"}";
                        column.Item().AlignCenter().Text(dateRange).FontSize(9);
                    }
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    // Summary Stats
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("Total Sessions").FontSize(9).Bold();
                            col.Item().Text(data.TotalScheduledSessions.ToString()).FontSize(16).FontColor("#0d6efd");
                        });
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("Attended").FontSize(9).Bold();
                            col.Item().Text(data.SessionsAttended.ToString()).FontSize(16).FontColor("#198754");
                        });
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("Missed").FontSize(9).Bold();
                            col.Item().Text(data.SessionsMissed.ToString()).FontSize(16).FontColor("#dc3545");
                        });
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("Late").FontSize(9).Bold();
                            col.Item().Text(data.LateArrivals.ToString()).FontSize(16).FontColor("#ffc107");
                        });
                        table.Cell().Border(1).Padding(8).Column(col =>
                        {
                            col.Item().Text("Attendance %").FontSize(9).Bold();
                            col.Item().Text($"{data.AttendancePercentage:F1}%").FontSize(16).FontColor("#6f42c1");
                        });
                    });

                    // Session Details
                    column.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70); // Date
                            columns.RelativeColumn(2); // Course
                            columns.ConstantColumn(50); // Code
                            columns.ConstantColumn(60); // Room
                            columns.ConstantColumn(50); // Start
                            columns.ConstantColumn(50); // End
                            columns.ConstantColumn(60); // Status
                            columns.ConstantColumn(50); // Late
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Date").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Course").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Code").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Room").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Start").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("End").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Status").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Late").FontSize(9);
                        });

                        int rowNum = 0;
                        foreach (var session in data.Sessions)
                        {
                            var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                            table.Cell().Background(bgColor).Padding(5).Text(session.SessionDate.ToString("MM/dd/yyyy")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.CourseName).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.CourseCode).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.RoomName).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.ScheduledStartTime.ToString(@"hh\:mm")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.ScheduledEndTime.ToString(@"hh\:mm")).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.Status).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(session.MinutesLate.HasValue ? $"{session.MinutesLate}m" : "-").FontSize(8);

                            rowNum++;
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateTeacherAttendanceExcelAsync(
        int teacherId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var data = await GenerateTeacherAttendanceDataAsync(teacherId, startDate, endDate);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Teacher Attendance");

        // Title
        worksheet.Cells["A1"].Value = "Teacher Attendance Report";
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        // Teacher info
        worksheet.Cells["A2"].Value = $"Teacher: {data.TeacherName}";
        worksheet.Cells["A3"].Value = $"Department: {data.DepartmentName}";

        // Summary
        worksheet.Cells["A5"].Value = "Total Sessions:";
        worksheet.Cells["B5"].Value = data.TotalScheduledSessions;
        worksheet.Cells["C5"].Value = "Attended:";
        worksheet.Cells["D5"].Value = data.SessionsAttended;
        worksheet.Cells["E5"].Value = "Missed:";
        worksheet.Cells["F5"].Value = data.SessionsMissed;
        worksheet.Cells["G5"].Value = "Late:";
        worksheet.Cells["H5"].Value = data.LateArrivals;
        worksheet.Cells["I5"].Value = "Attendance %:";
        worksheet.Cells["J5"].Value = $"{data.AttendancePercentage:F1}%";

        // Headers
        var headers = new[] { "Date", "Course", "Code", "Room", "Start Time", "End Time", "Status", "Minutes Late", "Check In", "Check Out" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[7, i + 1].Value = headers[i];
            worksheet.Cells[7, i + 1].Style.Font.Bold = true;
            worksheet.Cells[7, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[7, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 8;
        foreach (var session in data.Sessions)
        {
            worksheet.Cells[row, 1].Value = session.SessionDate.ToString("MM/dd/yyyy");
            worksheet.Cells[row, 2].Value = session.CourseName;
            worksheet.Cells[row, 3].Value = session.CourseCode;
            worksheet.Cells[row, 4].Value = session.RoomName;
            worksheet.Cells[row, 5].Value = session.ScheduledStartTime.ToString(@"hh\:mm");
            worksheet.Cells[row, 6].Value = session.ScheduledEndTime.ToString(@"hh\:mm");
            worksheet.Cells[row, 7].Value = session.Status;
            worksheet.Cells[row, 8].Value = session.MinutesLate.HasValue ? session.MinutesLate.ToString() : "-";
            worksheet.Cells[row, 9].Value = session.ActualCheckInTime?.ToString("HH:mm") ?? "-";
            worksheet.Cells[row, 10].Value = session.ActualCheckOutTime?.ToString("HH:mm") ?? "-";
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    #endregion

    #region Department Attendance Summary Report

    public async Task<DepartmentAttendanceSummaryDto> GenerateDepartmentAttendanceSummaryDataAsync(
        DateTime? startDate = null, DateTime? endDate = null, int? termId = null)
    {
        var result = new DepartmentAttendanceSummaryDto
        {
            GeneratedDate = DateTime.UtcNow,
            StartDate = startDate,
            EndDate = endDate,
            Departments = new List<DepartmentAttendanceItemDto>(),
            OverallStats = new OverallStatistics()
        };

        Models.Entities.Academic.Term? term = null;
        if (termId.HasValue)
        {
            term = await _unitOfWork.Terms.GetByIdAsync(termId.Value);
            result.TermName = term?.Name;
            if (term != null)
            {
                startDate = term.StartDate;
                endDate = term.EndDate;
            }
        }

        var departments = await _unitOfWork.Departments.GetAllAsync();
        int totalStudentsAtRisk = 0;
        int totalStudentsCount = 0;
        int totalPresentCount = 0;
        int totalAttendanceRecords = 0;

        foreach (var department in departments)
        {
            // Get students in department
            var students = await _unitOfWork.Students.GetByDepartmentAsync(department.Id);
            var studentIds = students.Select(s => s.Id).ToList();

            // Get all course offerings and filter by department through Course navigation
            var allOfferings = await _unitOfWork.CourseOfferings.GetAllAsync();
            var offerings = allOfferings.Where(o => o.Course.DepartmentId == department.Id);
            var offeringIds = offerings.Select(o => o.Id).ToList();

            // Get sessions within date range
            var allSessions = new List<Models.Entities.Scheduling.Session>();
            foreach (var offering in offerings)
            {
                var sessions = offering.Sessions
                    .Where(s => (!startDate.HasValue || s.SessionDate >= startDate.Value) &&
                               (!endDate.HasValue || s.SessionDate <= endDate.Value))
                    .ToList();
                allSessions.AddRange(sessions);
            }

            var sessionIds = allSessions.Select(s => s.Id).ToList();

            // Get attendance records for these students and sessions
            var allAttendances = await _unitOfWork.Attendances.GetAllAsync();
            var attendances = allAttendances
                .Where(a => studentIds.Contains(a.StudentId) && sessionIds.Contains(a.SessionId))
                .ToList();

            var presentCount = attendances.Count(a => a.Status == "Present");
            var absentCount = attendances.Count(a => a.Status == "Absent");
            var lateCount = attendances.Count(a => a.Status == "Late");
            var excusedCount = attendances.Count(a => a.Status == "Excused");

            var overallAttendancePercentage = attendances.Count > 0
                ? (decimal)(presentCount + lateCount + excusedCount) / attendances.Count * 100
                : 0;

            // Count at-risk students (< 75% attendance)
            int studentsAtRisk = 0;
            foreach (var student in students)
            {
                var studentAttendances = attendances.Where(a => a.StudentId == student.Id).ToList();
                var studentSessions = allSessions.Where(s => offeringIds.Contains(s.CourseOfferingId)).Count();

                if (studentSessions > 0)
                {
                    var studentPresent = studentAttendances.Count(a => a.Status == "Present" || a.Status == "Late" || a.Status == "Excused");
                    var studentPercentage = (decimal)studentPresent / studentSessions * 100;

                    if (studentPercentage < 75)
                        studentsAtRisk++;
                }
            }

            result.Departments.Add(new DepartmentAttendanceItemDto
            {
                DepartmentId = department.Id,
                DepartmentName = department.Name,
                TotalStudents = students.Count(),
                TotalSessions = allSessions.Count,
                TotalAttendanceRecords = attendances.Count,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                LateCount = lateCount,
                ExcusedCount = excusedCount,
                OverallAttendancePercentage = overallAttendancePercentage,
                StudentsAtRisk = studentsAtRisk
            });

            totalStudentsAtRisk += studentsAtRisk;
            totalStudentsCount += students.Count();
            totalPresentCount += presentCount + lateCount + excusedCount;
            totalAttendanceRecords += attendances.Count;
        }

        result.OverallStats = new OverallStatistics
        {
            TotalDepartments = departments.Count(),
            TotalStudents = totalStudentsCount,
            SystemWideAttendancePercentage = totalAttendanceRecords > 0
                ? (decimal)totalPresentCount / totalAttendanceRecords * 100
                : 0,
            TotalStudentsAtRisk = totalStudentsAtRisk
        };

        return result;
    }

    public async Task<byte[]> GenerateDepartmentAttendanceSummaryPdfAsync(
        DateTime? startDate = null, DateTime? endDate = null, int? termId = null)
    {
        var data = await GenerateDepartmentAttendanceSummaryDataAsync(startDate, endDate, termId);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("Department Attendance Summary Report")
                        .FontSize(18).Bold().FontColor("#20c997");
                    if (!string.IsNullOrEmpty(data.TermName))
                    {
                        column.Item().PaddingTop(5).AlignCenter().Text($"Term: {data.TermName}")
                            .FontSize(12);
                    }
                    if (data.StartDate.HasValue || data.EndDate.HasValue)
                    {
                        var dateRange = $"Period: {data.StartDate?.ToString("MMM dd, yyyy") ?? "Start"} - {data.EndDate?.ToString("MMM dd, yyyy") ?? "End"}";
                        column.Item().AlignCenter().Text(dateRange).FontSize(9);
                    }
                });

                page.Content().PaddingTop(20).Column(column =>
                {
                    // Overall Statistics
                    column.Item().Background("#e7f3ff").Border(1).BorderColor("#0d6efd").Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Total Departments").FontSize(10);
                            col.Item().Text(data.OverallStats.TotalDepartments.ToString()).FontSize(18).Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Total Students").FontSize(10);
                            col.Item().Text(data.OverallStats.TotalStudents.ToString()).FontSize(18).Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("System-Wide Attendance").FontSize(10);
                            col.Item().Text($"{data.OverallStats.SystemWideAttendancePercentage:F1}%").FontSize(18).Bold();
                        });
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Students At Risk (<75%)").FontSize(10);
                            col.Item().Text(data.OverallStats.TotalStudentsAtRisk.ToString()).FontSize(18).Bold().FontColor("#dc3545");
                        });
                    });

                    // Department Details
                    column.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(70);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background("#f8f9fa").Padding(5).Text("#").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Department").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Students").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Sessions").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Records").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Present").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Absent").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Late").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Excused").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("Attendance %").FontSize(9);
                            header.Cell().Background("#f8f9fa").Padding(5).Text("At Risk").FontSize(9);
                        });

                        int rowNum = 1;
                        foreach (var dept in data.Departments)
                        {
                            var bgColor = rowNum % 2 == 0 ? "#ffffff" : "#f8f9fa";

                            table.Cell().Background(bgColor).Padding(5).Text(rowNum.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.DepartmentName).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.TotalStudents.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.TotalSessions.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.TotalAttendanceRecords.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.PresentCount.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.AbsentCount.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.LateCount.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.ExcusedCount.ToString()).FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text($"{dept.OverallAttendancePercentage:F1}%").FontSize(8);
                            table.Cell().Background(bgColor).Padding(5).Text(dept.StudentsAtRisk.ToString()).FontSize(8);

                            rowNum++;
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on: ");
                    text.Span(data.GeneratedDate.ToString("MMM dd, yyyy HH:mm")).Bold();
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateDepartmentAttendanceSummaryExcelAsync(
        DateTime? startDate = null, DateTime? endDate = null, int? termId = null)
    {
        var data = await GenerateDepartmentAttendanceSummaryDataAsync(startDate, endDate, termId);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Department Summary");

        // Title
        worksheet.Cells["A1"].Value = "Department Attendance Summary Report";
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        if (!string.IsNullOrEmpty(data.TermName))
        {
            worksheet.Cells["A2"].Value = $"Term: {data.TermName}";
        }

        // Overall Stats
        worksheet.Cells["A4"].Value = "System-Wide Statistics";
        worksheet.Cells["A4"].Style.Font.Bold = true;
        worksheet.Cells["A5"].Value = "Total Departments:";
        worksheet.Cells["B5"].Value = data.OverallStats.TotalDepartments;
        worksheet.Cells["C5"].Value = "Total Students:";
        worksheet.Cells["D5"].Value = data.OverallStats.TotalStudents;
        worksheet.Cells["E5"].Value = "System-Wide Attendance:";
        worksheet.Cells["F5"].Value = $"{data.OverallStats.SystemWideAttendancePercentage:F1}%";
        worksheet.Cells["G5"].Value = "Students At Risk:";
        worksheet.Cells["H5"].Value = data.OverallStats.TotalStudentsAtRisk;

        // Headers
        var headers = new[] { "#", "Department", "Students", "Sessions", "Records", "Present", "Absent", "Late", "Excused", "Attendance %", "At Risk" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[7, i + 1].Value = headers[i];
            worksheet.Cells[7, i + 1].Style.Font.Bold = true;
            worksheet.Cells[7, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[7, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 8;
        int num = 1;
        foreach (var dept in data.Departments)
        {
            worksheet.Cells[row, 1].Value = num++;
            worksheet.Cells[row, 2].Value = dept.DepartmentName;
            worksheet.Cells[row, 3].Value = dept.TotalStudents;
            worksheet.Cells[row, 4].Value = dept.TotalSessions;
            worksheet.Cells[row, 5].Value = dept.TotalAttendanceRecords;
            worksheet.Cells[row, 6].Value = dept.PresentCount;
            worksheet.Cells[row, 7].Value = dept.AbsentCount;
            worksheet.Cells[row, 8].Value = dept.LateCount;
            worksheet.Cells[row, 9].Value = dept.ExcusedCount;
            worksheet.Cells[row, 10].Value = $"{dept.OverallAttendancePercentage:F1}%";
            worksheet.Cells[row, 11].Value = dept.StudentsAtRisk;
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    #endregion
}
