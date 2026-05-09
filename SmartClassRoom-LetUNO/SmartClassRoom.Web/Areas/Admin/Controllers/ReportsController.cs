using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Common;
using SmartClassRoom.Web.Models.ViewModels.Common;
using SmartClassRoom.Web.Models.ViewModels.Reports;
using SmartClassRoom.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartClassRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Teacher")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IReportService reportService, IUnitOfWork unitOfWork)
    {
        _reportService = reportService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int? termId = null,
        string? reportType = null,
        string? sortBy = null,
        bool ascending = false)
    {
        const int pageSize = 20;

        var reports = await _reportService.GetAvailableReportsAsync(termId, reportType);

        var sorted = sortBy?.ToLower() switch
        {
            "course" => ascending ? reports.OrderBy(r => r.CourseCode) : reports.OrderByDescending(r => r.CourseCode),
            "term" => ascending ? reports.OrderBy(r => r.TermName) : reports.OrderByDescending(r => r.TermName),
            "type" => ascending ? reports.OrderBy(r => r.Type) : reports.OrderByDescending(r => r.Type),
            _ => reports.OrderByDescending(r => r.TermName).ThenBy(r => r.CourseCode)
        };

        var totalCount = sorted.Count();
        var paginatedReports = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var pagedResult = new PagedResult<ReportListItemDto>
        {
            Items = paginatedReports,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        var paginationViewModel = PaginationViewModel.Create(
            pagedResult,
            routeValues: new Dictionary<string, string?>
            {
                { "termId", termId?.ToString() },
                { "reportType", reportType },
                { "sortBy", sortBy },
                { "ascending", ascending.ToString() }
            },
            actionName: "Index",
            controllerName: "Reports",
            areaName: "Admin");

        ViewBag.Pagination = paginationViewModel;
        ViewBag.SelectedTermId = termId;
        ViewBag.SelectedReportType = reportType;
        ViewBag.CurrentSort = sortBy;
        ViewBag.SortAscending = ascending;

        var terms = await _unitOfWork.Terms.GetAllAsync();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();

        return View(paginatedReports);
    }

    [HttpGet]
    public async Task<IActionResult> AttendanceReport(int id)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);
        if (offering == null)
            return NotFound();

        ViewBag.Offering = offering;
        ViewBag.Terms = await _unitOfWork.Terms.GetAllAsync();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateAttendancePdf(
        int courseOfferingId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateAttendanceReportPdfAsync(
                courseOfferingId, dateFrom, dateTo);

            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"Attendance_{offering?.Course.Code ?? "Report"}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateAttendanceExcel(
        int courseOfferingId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        try
        {
            var excelBytes = await _reportService.GenerateAttendanceReportExcelAsync(
                courseOfferingId, dateFrom, dateTo);

            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"Attendance_{offering?.Course.Code ?? "Report"}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GradeReport(int id)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(id);
        if (offering == null)
            return NotFound();

        var hasComponents = offering.GradeComponents?.Any() ?? false;
        if (!hasComponents)
        {
            TempData["ErrorMessage"] = "No grade components configured for this course.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Offering = offering;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateGradePdf(int courseOfferingId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateGradeReportPdfAsync(courseOfferingId);

            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"Grades_{offering?.Course.Code ?? "Report"}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateGradeExcel(int courseOfferingId)
    {
        try
        {
            var excelBytes = await _reportService.GenerateGradeReportExcelAsync(courseOfferingId);

            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"Grades_{offering?.Course.Code ?? "Report"}_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> EnrollmentReport(int? termId = null)
    {
        var terms = await _unitOfWork.Terms.GetAllAsync();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();
        ViewBag.SelectedTermId = termId;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateEnrollmentPdf(int? termId = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateEnrollmentReportPdfAsync(termId);

            var term = termId.HasValue
                ? await _unitOfWork.Terms.GetByIdAsync(termId.Value)
                : null;
            var fileName = term != null
                ? $"Enrollment_{term.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
                : $"Enrollment_All_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateEnrollmentExcel(int? termId = null)
    {
        try
        {
            var excelBytes = await _reportService.GenerateEnrollmentReportExcelAsync(termId);

            var term = termId.HasValue
                ? await _unitOfWork.Terms.GetByIdAsync(termId.Value)
                : null;
            var fileName = term != null
                ? $"Enrollment_{term.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx"
                : $"Enrollment_All_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> StudentListReport()
    {
        var terms = await _unitOfWork.Terms.GetAllAsync();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();

        var departments = await _unitOfWork.Departments.GetAllAsync();
        ViewBag.Departments = departments.OrderBy(d => d.Name).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateStudentListPdf(
        int? termId = null,
        string? level = null,
        string? department = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateStudentListReportPdfAsync(termId, level, department);

            var fileName = $"StudentList_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateStudentReportPdf(int studentId)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateStudentReportPdfAsync(studentId);

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            var fileName = $"StudentReport_{student?.StudentCode ?? studentId.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Student Multi-Course Attendance Report

    [HttpGet]
    public async Task<IActionResult> StudentMultiCourseAttendanceReport()
    {
        var students = await _unitOfWork.Students.GetAllAsync();
        var terms = await _unitOfWork.Terms.GetAllAsync();
        var offerings = await _unitOfWork.CourseOfferings.GetAllAsync("Course", "Term", "Teacher");
        ViewBag.Students = students.OrderBy(s => s.FullName).ToList();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();
        ViewBag.CourseOfferings = offerings.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> StudentMultiCourseAttendanceReport(int studentId, List<int> courseOfferingIds)
    {
        try
        {
            var data = await _reportService.GenerateStudentMultiCourseAttendanceDataAsync(studentId, courseOfferingIds);
            return View("StudentMultiCourseAttendanceResult", data);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(StudentMultiCourseAttendanceReport));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateStudentMultiCourseAttendancePdf(int studentId, [FromQuery] List<int> courseOfferingIds)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateStudentMultiCourseAttendancePdfAsync(studentId, courseOfferingIds);
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            var fileName = $"StudentMultiCourseAttendance_{student?.StudentCode ?? studentId.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(StudentMultiCourseAttendanceReport));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateStudentMultiCourseAttendanceExcel(int studentId, [FromQuery] List<int> courseOfferingIds)
    {
        try
        {
            var excelBytes = await _reportService.GenerateStudentMultiCourseAttendanceExcelAsync(studentId, courseOfferingIds);
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            var fileName = $"StudentMultiCourseAttendance_{student?.StudentCode ?? studentId.ToString()}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(StudentMultiCourseAttendanceReport));
        }
    }

    #endregion

    #region Course Student Attendance Report

    [HttpGet]
    public async Task<IActionResult> CourseStudentAttendanceReport()
    {
        var offerings = await _unitOfWork.CourseOfferings.GetAllAsync("Course", "Term", "Teacher");
        ViewBag.CourseOfferings = offerings.OrderBy(o => o.Course.Name).ToList();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateCourseStudentAttendancePdf(int courseOfferingId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateCourseStudentAttendancePdfAsync(courseOfferingId, startDate, endDate);
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"CourseStudentAttendance_{offering?.Course?.Code ?? courseOfferingId.ToString()}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(CourseStudentAttendanceReport));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateCourseStudentAttendanceExcel(int courseOfferingId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var excelBytes = await _reportService.GenerateCourseStudentAttendanceExcelAsync(courseOfferingId, startDate, endDate);
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
            var fileName = $"CourseStudentAttendance_{offering?.Course?.Code ?? courseOfferingId.ToString()}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(CourseStudentAttendanceReport));
        }
    }

    #endregion

    #region Teacher Attendance Report

    [HttpGet]
    public async Task<IActionResult> TeacherAttendanceReport()
    {
        var teachers = await _unitOfWork.Teachers.GetAllAsync();
        ViewBag.Teachers = teachers.OrderBy(t => t.FullName).ToList();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateTeacherAttendancePdf(int teacherId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateTeacherAttendancePdfAsync(teacherId, startDate, endDate);
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
            var fileName = $"TeacherAttendance_{teacher?.FullName.Replace(" ", "")}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(TeacherAttendanceReport));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateTeacherAttendanceExcel(int teacherId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var excelBytes = await _reportService.GenerateTeacherAttendanceExcelAsync(teacherId, startDate, endDate);
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
            var fileName = $"TeacherAttendance_{teacher?.FullName.Replace(" ", "")}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(TeacherAttendanceReport));
        }
    }

    #endregion

    #region Department Attendance Summary Report

    [HttpGet]
    public async Task<IActionResult> DepartmentAttendanceSummaryReport()
    {
        var terms = await _unitOfWork.Terms.GetAllAsync();
        ViewBag.Terms = terms.OrderByDescending(t => t.StartDate).ToList();
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GenerateDepartmentAttendanceSummaryPdf(DateTime? startDate = null, DateTime? endDate = null, int? termId = null)
    {
        try
        {
            var pdfBytes = await _reportService.GenerateDepartmentAttendanceSummaryPdfAsync(startDate, endDate, termId);
            var fileName = $"DepartmentAttendanceSummary_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(DepartmentAttendanceSummaryReport));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateDepartmentAttendanceSummaryExcel(DateTime? startDate = null, DateTime? endDate = null, int? termId = null)
    {
        try
        {
            var excelBytes = await _reportService.GenerateDepartmentAttendanceSummaryExcelAsync(startDate, endDate, termId);
            var fileName = $"DepartmentAttendanceSummary_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(DepartmentAttendanceSummaryReport));
        }
    }

    #endregion

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
