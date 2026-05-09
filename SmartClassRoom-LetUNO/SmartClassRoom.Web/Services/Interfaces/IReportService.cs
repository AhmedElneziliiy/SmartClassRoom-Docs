using SmartClassRoom.Web.Models.ViewModels.Reports;

namespace SmartClassRoom.Web.Services.Interfaces;

public interface IReportService
{
    // Attendance Reports
    Task<AttendanceReportDto> GenerateAttendanceReportDataAsync(
        int courseOfferingId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);

    Task<byte[]> GenerateAttendanceReportPdfAsync(
        int courseOfferingId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);

    Task<byte[]> GenerateAttendanceReportExcelAsync(
        int courseOfferingId,
        DateTime? dateFrom = null,
        DateTime? dateTo = null);

    // Grade Reports
    Task<GradeReportDto> GenerateGradeReportDataAsync(int courseOfferingId);

    Task<byte[]> GenerateGradeReportPdfAsync(int courseOfferingId);

    Task<byte[]> GenerateGradeReportExcelAsync(int courseOfferingId);

    // Enrollment Reports
    Task<EnrollmentReportDto> GenerateEnrollmentReportDataAsync(int? termId = null);

    Task<byte[]> GenerateEnrollmentReportPdfAsync(int? termId = null);

    Task<byte[]> GenerateEnrollmentReportExcelAsync(int? termId = null);

    // List available reports
    Task<List<ReportListItemDto>> GetAvailableReportsAsync(
        int? termId = null,
        string? reportType = null);

    // Student Reports
    Task<StudentReportDto> GenerateStudentReportDataAsync(int studentId);

    Task<byte[]> GenerateStudentReportPdfAsync(int studentId);

    Task<StudentListReportDto> GenerateStudentListReportDataAsync(
        int? termId = null,
        string? level = null,
        string? department = null);

    Task<byte[]> GenerateStudentListReportPdfAsync(
        int? termId = null,
        string? level = null,
        string? department = null);

    // Student Multi-Course Attendance Report
    Task<StudentMultiCourseAttendanceDto> GenerateStudentMultiCourseAttendanceDataAsync(
        int studentId,
        List<int> courseOfferingIds);

    Task<byte[]> GenerateStudentMultiCourseAttendancePdfAsync(
        int studentId,
        List<int> courseOfferingIds);

    Task<byte[]> GenerateStudentMultiCourseAttendanceExcelAsync(
        int studentId,
        List<int> courseOfferingIds);

    // Course Student Attendance Report (Session-by-Session)
    Task<CourseStudentAttendanceDto> GenerateCourseStudentAttendanceDataAsync(
        int courseOfferingId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<byte[]> GenerateCourseStudentAttendancePdfAsync(
        int courseOfferingId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<byte[]> GenerateCourseStudentAttendanceExcelAsync(
        int courseOfferingId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    // Teacher Attendance Report
    Task<TeacherAttendanceDto> GenerateTeacherAttendanceDataAsync(
        int teacherId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<byte[]> GenerateTeacherAttendancePdfAsync(
        int teacherId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    Task<byte[]> GenerateTeacherAttendanceExcelAsync(
        int teacherId,
        DateTime? startDate = null,
        DateTime? endDate = null);

    // Department Attendance Summary Report
    Task<DepartmentAttendanceSummaryDto> GenerateDepartmentAttendanceSummaryDataAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? termId = null);

    Task<byte[]> GenerateDepartmentAttendanceSummaryPdfAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? termId = null);

    Task<byte[]> GenerateDepartmentAttendanceSummaryExcelAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? termId = null);
}
