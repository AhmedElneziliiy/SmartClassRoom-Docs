using Microsoft.EntityFrameworkCore;
using SmartClassRoom.Web.Data;
using SmartClassRoom.Web.Data.Repositories.Interfaces;
using SmartClassRoom.Web.Models.Constants;
using SmartClassRoom.Web.Models.Entities.Grading;
using SmartClassRoom.Web.Models.ViewModels.Grading;
using SmartClassRoom.Web.Services.Interfaces;

namespace SmartClassRoom.Web.Services.Implementations;

public class GradingService : IGradingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public GradingService(IUnitOfWork unitOfWork, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<(bool Success, string Message)> SaveGradeComponentsAsync(int courseOfferingId, List<GradeComponentDto> components)
    {
        // Validate offering exists
        var offering = await _unitOfWork.CourseOfferings.GetByIdAsync(courseOfferingId);
        if (offering == null)
            return (false, "Course offering not found");

        // Validate weights sum to 100%
        var totalWeight = components.Sum(c => c.Weight);
        if (totalWeight != 100)
            return (false, $"Component weights must sum to 100%. Current total: {totalWeight}%");

        // Validate no duplicate component names
        var duplicates = components.GroupBy(c => c.ComponentName.Trim().ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            return (false, $"Duplicate component names: {string.Join(", ", duplicates)}");

        // Check if there are existing grades (warn user in UI if needed)
        var existingGradesCount = await _context.Grades
            .CountAsync(g => g.CourseOfferingId == courseOfferingId);

        // Delete existing components and their associated grades
        await _unitOfWork.GradeComponents.DeleteComponentsByOfferingAsync(courseOfferingId);

        // Create new components
        var newComponents = components.Select((c, index) => new GradeComponent
        {
            CourseOfferingId = courseOfferingId,
            ComponentName = c.ComponentName.Trim(),
            Weight = c.Weight,
            MaxScore = c.MaxScore,
            OrderIndex = index,
            Description = c.Description?.Trim()
        }).ToList();

        foreach (var component in newComponents)
        {
            await _unitOfWork.GradeComponents.AddAsync(component);
        }

        await _unitOfWork.SaveChangesAsync();

        var message = $"{newComponents.Count} grade components saved successfully";
        if (existingGradesCount > 0)
        {
            message += $". Warning: {existingGradesCount} existing grade(s) were deleted.";
        }

        return (true, message);
    }

    public async Task<IEnumerable<GradeComponentDto>> GetGradeComponentsAsync(int courseOfferingId)
    {
        var components = await _unitOfWork.GradeComponents.GetComponentsByOfferingAsync(courseOfferingId);

        return components.Select(c => new GradeComponentDto
        {
            Id = c.Id,
            ComponentName = c.ComponentName,
            Weight = c.Weight,
            MaxScore = c.MaxScore,
            OrderIndex = c.OrderIndex,
            Description = c.Description
        }).ToList();
    }

    public async Task<(bool Success, string Message)> SaveGradeAsync(SaveGradeDto gradeDto)
    {
        // Validate component exists
        var component = await _unitOfWork.GradeComponents.GetByIdAsync(gradeDto.GradeComponentId);
        if (component == null)
            return (false, "Grade component not found");

        // Validate student enrolled (accepts both Enrolled and Completed statuses)
        var enrollment = await _unitOfWork.StudentEnrollments.GetEnrollmentAsync(
            gradeDto.StudentId, component.CourseOfferingId);

        if (enrollment == null || !EnrollmentStatus.IsActive(enrollment.Status))
            return (false, "Student is not enrolled in this course offering");

        // Validate score range
        if (gradeDto.Score < 0 || gradeDto.Score > component.MaxScore)
            return (false, $"Score must be between 0 and {component.MaxScore}");

        // Check if grade exists
        var existingGrade = await _unitOfWork.Grades.GetGradeAsync(
            gradeDto.StudentId, component.CourseOfferingId, gradeDto.GradeComponentId);

        if (existingGrade != null)
        {
            // Update existing grade
            existingGrade.Score = gradeDto.Score;
            existingGrade.Feedback = gradeDto.Feedback?.Trim();
            existingGrade.UpdatedAt = DateTime.UtcNow;
            existingGrade.EnteredBy = gradeDto.EnteredBy;

            _unitOfWork.Grades.Update(existingGrade);
        }
        else
        {
            // Create new grade
            var newGrade = new Grade
            {
                StudentId = gradeDto.StudentId,
                CourseOfferingId = component.CourseOfferingId,
                GradeComponentId = gradeDto.GradeComponentId,
                Score = gradeDto.Score,
                Feedback = gradeDto.Feedback?.Trim(),
                EnteredBy = gradeDto.EnteredBy,
                EnteredAt = DateTime.UtcNow
            };

            await _unitOfWork.Grades.AddAsync(newGrade);
        }

        await _unitOfWork.SaveChangesAsync();

        return (true, "Grade saved successfully");
    }

    public async Task<StudentGradeDto?> GetStudentGradesAsync(int studentId, int courseOfferingId)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null) return null;

        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null) return null;

        var components = await _unitOfWork.GradeComponents.GetComponentsByOfferingAsync(courseOfferingId);
        var grades = await _unitOfWork.Grades.GetGradesByStudentAndOfferingAsync(studentId, courseOfferingId);

        var componentGrades = components.Select(c =>
        {
            var grade = grades.FirstOrDefault(g => g.GradeComponentId == c.Id);
            return new ComponentGradeDto
            {
                ComponentId = c.Id,
                ComponentName = c.ComponentName,
                Weight = c.Weight,
                MaxScore = c.MaxScore,
                Score = grade?.Score,
                Feedback = grade?.Feedback,
                EnteredAt = grade?.EnteredAt
            };
        }).ToList();

        var enrollment = await _unitOfWork.StudentEnrollments.GetEnrollmentAsync(studentId, courseOfferingId);

        return new StudentGradeDto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            CourseOfferingId = courseOfferingId,
            CourseName = offering.Course.Name,
            ComponentGrades = componentGrades,
            FinalGrade = enrollment?.FinalGrade,
            LetterGrade = enrollment?.LetterGrade
        };
    }

    public async Task<(bool Success, string Message, FinalGradeCalculationResult? Result)> CalculateFinalGradesAsync(int courseOfferingId)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
            return (false, "Course offering not found", null);

        var components = await _unitOfWork.GradeComponents.GetComponentsByOfferingAsync(courseOfferingId);
        if (!components.Any())
            return (false, "No grade components configured for this offering", null);

        // Validate weights sum to 100%
        var totalWeight = components.Sum(c => c.Weight);
        if (totalWeight != 100)
            return (false, $"Component weights must sum to 100% before calculating final grades. Current: {totalWeight}%", null);

        var enrollments = await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentsAsync(courseOfferingId);
        var activeEnrollments = enrollments.Where(e => EnrollmentStatus.IsActive(e.Status)).ToList();

        if (!activeEnrollments.Any())
            return (false, "No enrolled students found", null);

        var result = new FinalGradeCalculationResult
        {
            TotalStudents = activeEnrollments.Count
        };

        foreach (var enrollment in activeEnrollments)
        {
            var grades = await _unitOfWork.Grades.GetGradesByStudentAndOfferingAsync(
                enrollment.StudentId, courseOfferingId);

            // Check if all components have grades
            var missingComponents = components.Where(c =>
                !grades.Any(g => g.GradeComponentId == c.Id)).ToList();

            if (missingComponents.Any())
            {
                result.FailedStudents++;
                result.Warnings.Add($"{enrollment.Student.FullName}: Missing grades for {string.Join(", ", missingComponents.Select(c => c.ComponentName))}");
                continue;
            }

            // Calculate weighted final score
            decimal finalScore = 0;
            foreach (var component in components)
            {
                var grade = grades.First(g => g.GradeComponentId == component.Id);
                // Normalize score to percentage: (score / maxScore) * weight
                finalScore += (grade.Score / component.MaxScore) * component.Weight;
            }

            // Convert to letter grade
            var (letterGrade, gradePoint) = GetLetterGrade(finalScore);

            // Update enrollment
            enrollment.FinalGrade = Math.Round(finalScore, 2);
            enrollment.LetterGrade = letterGrade;
            enrollment.Status = "Completed";

            _unitOfWork.StudentEnrollments.Update(enrollment);

            result.SuccessfulStudents++;
        }

        await _unitOfWork.SaveChangesAsync();

        // Update cumulative GPA for all students
        foreach (var enrollment in activeEnrollments)
        {
            await UpdateStudentCumulativeGPAAsync(enrollment.StudentId);
        }

        result.Message = $"Final grades calculated for {result.SuccessfulStudents} students";
        if (result.FailedStudents > 0)
        {
            result.Message += $". {result.FailedStudents} students have incomplete grades";
        }

        return (true, result.Message, result);
    }

    public async Task<GPADto?> CalculateGPAAsync(int studentId, int? termId = null)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null) return null;

        // Get all completed enrollments with final grades
        var allEnrollments = await _unitOfWork.StudentEnrollments.GetStudentEnrollmentsAsync(studentId);
        var completedEnrollments = allEnrollments
            .Where(e => e.Status == "Completed" && e.FinalGrade.HasValue && !string.IsNullOrEmpty(e.LetterGrade))
            .ToList();

        if (!completedEnrollments.Any())
        {
            return new GPADto
            {
                StudentId = studentId,
                StudentName = student.FullName,
                CumulativeGPA = 0,
                TotalCredits = 0,
                TotalQualityPoints = 0
            };
        }

        // Calculate cumulative GPA
        decimal totalQualityPoints = 0;
        int totalCredits = 0;

        foreach (var enrollment in completedEnrollments)
        {
            var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);
            if (offering?.Course == null) continue;

            var gradePoint = GetGradePoint(enrollment.LetterGrade!);
            var credits = offering.Course.Credits;

            totalQualityPoints += gradePoint * credits;
            totalCredits += credits;
        }

        var cumulativeGPA = totalCredits > 0 ? Math.Round(totalQualityPoints / totalCredits, 2) : 0;

        // Calculate term GPA if termId provided
        decimal? termGPA = null;
        int? termCredits = null;
        if (termId.HasValue)
        {
            var termEnrollments = completedEnrollments
                .Where(e => e.CourseOffering.TermId == termId.Value)
                .ToList();

            if (termEnrollments.Any())
            {
                decimal termQualityPoints = 0;
                int termCreditCount = 0;

                foreach (var enrollment in termEnrollments)
                {
                    var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(enrollment.CourseOfferingId);
                    if (offering?.Course == null) continue;

                    var gradePoint = GetGradePoint(enrollment.LetterGrade!);
                    var credits = offering.Course.Credits;

                    termQualityPoints += gradePoint * credits;
                    termCreditCount += credits;
                }

                termGPA = termCreditCount > 0 ? Math.Round(termQualityPoints / termCreditCount, 2) : 0;
                termCredits = termCreditCount;
            }
        }

        return new GPADto
        {
            StudentId = studentId,
            StudentName = student.FullName,
            CumulativeGPA = cumulativeGPA,
            TermGPA = termGPA,
            TotalCredits = totalCredits,
            TermCredits = termCredits,
            TotalQualityPoints = totalQualityPoints,
            TermId = termId
        };
    }

    public async Task UpdateStudentCumulativeGPAAsync(int studentId)
    {
        var gpaDto = await CalculateGPAAsync(studentId);
        if (gpaDto == null) return;

        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student == null) return;

        student.CurrentGPA = gpaDto.CumulativeGPA;
        _unitOfWork.Students.Update(student);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<GradeSheetDto> GetGradeSheetAsync(int courseOfferingId)
    {
        var offering = await _unitOfWork.CourseOfferings.GetOfferingWithDetailsAsync(courseOfferingId);
        if (offering == null)
            throw new ArgumentException("Course offering not found");

        var components = await _unitOfWork.GradeComponents.GetComponentsByOfferingAsync(courseOfferingId);
        var enrollments = await _unitOfWork.StudentEnrollments.GetOfferingEnrollmentsAsync(courseOfferingId);
        var activeEnrollments = enrollments.Where(e => EnrollmentStatus.IsActive(e.Status)).ToList();

        var studentRows = new List<GradeSheetStudentDto>();

        foreach (var enrollment in activeEnrollments)
        {
            var grades = await _unitOfWork.Grades.GetGradesByStudentAndOfferingAsync(
                enrollment.StudentId, courseOfferingId);

            var componentScores = components.Select(c =>
            {
                var grade = grades.FirstOrDefault(g => g.GradeComponentId == c.Id);
                return new GradeSheetComponentScoreDto
                {
                    ComponentId = c.Id,
                    Score = grade?.Score,
                    GradeId = grade?.Id
                };
            }).ToList();

            studentRows.Add(new GradeSheetStudentDto
            {
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student.FullName,
                StudentCode = enrollment.Student.StudentCode,
                ComponentScores = componentScores,
                FinalGrade = enrollment.FinalGrade,
                LetterGrade = enrollment.LetterGrade
            });
        }

        return new GradeSheetDto
        {
            CourseOfferingId = courseOfferingId,
            CourseName = offering.Course.Name,
            Code = offering.Course.Code,
            TermName = offering.Term.Name,
            Components = components.Select(c => new GradeComponentDto
            {
                Id = c.Id,
                ComponentName = c.ComponentName,
                Weight = c.Weight,
                MaxScore = c.MaxScore,
                OrderIndex = c.OrderIndex
            }).ToList(),
            Students = studentRows
        };
    }

    public async Task<(bool Success, string Message, int? TemplateId)> SaveTemplateAsync(SaveTemplateRequest request)
    {
        // Validate weights
        var totalWeight = request.Components.Sum(c => c.Weight);
        if (totalWeight != 100)
            return (false, $"Component weights must sum to 100%. Current: {totalWeight}%", null);

        // Check for duplicate name
        var exists = await _unitOfWork.GradeComponentTemplates.TemplateNameExistsAsync(
            request.TemplateName, request.DepartmentId, request.TemplateId);

        if (exists)
            return (false, "A template with this name already exists", null);

        GradeComponentTemplate template;

        if (request.TemplateId.HasValue)
        {
            // Update existing
            template = await _unitOfWork.GradeComponentTemplates.GetTemplateWithItemsAsync(request.TemplateId.Value);
            if (template == null)
                return (false, "Template not found", null);

            template.TemplateName = request.TemplateName;
            template.Description = request.Description;
            template.UpdatedAt = DateTime.UtcNow;

            // Remove old items
            _context.GradeComponentTemplateItems.RemoveRange(template.Items);
        }
        else
        {
            // Create new
            template = new GradeComponentTemplate
            {
                TemplateName = request.TemplateName,
                Description = request.Description,
                DepartmentId = request.DepartmentId,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.GradeComponentTemplates.AddAsync(template);
        }

        // Add items
        var items = request.Components.Select((c, index) => new GradeComponentTemplateItem
        {
            Template = template,
            ComponentName = c.ComponentName,
            Weight = c.Weight,
            MaxScore = c.MaxScore,
            OrderIndex = index,
            Description = c.Description
        }).ToList();

        template.Items = items;

        await _unitOfWork.SaveChangesAsync();

        return (true, "Template saved successfully", template.Id);
    }

    public async Task<IEnumerable<GradeComponentTemplateDto>> GetTemplatesAsync(int? departmentId = null)
    {
        var templates = await _unitOfWork.GradeComponentTemplates.GetActiveTemplatesAsync(departmentId);

        return templates.Select(t => new GradeComponentTemplateDto
        {
            Id = t.Id,
            TemplateName = t.TemplateName,
            Description = t.Description,
            DepartmentId = t.DepartmentId,
            DepartmentName = t.Department?.Name,
            ComponentCount = t.Items.Count,
            Components = t.Items.Select(i => new GradeComponentDto
            {
                ComponentName = i.ComponentName,
                Weight = i.Weight,
                MaxScore = i.MaxScore,
                OrderIndex = i.OrderIndex,
                Description = i.Description
            }).ToList()
        }).ToList();
    }

    public async Task<(bool Success, string Message)> ApplyTemplateAsync(int courseOfferingId, int templateId)
    {
        var template = await _unitOfWork.GradeComponentTemplates.GetTemplateWithItemsAsync(templateId);
        if (template == null || !template.IsActive)
            return (false, "Template not found");

        var components = template.Items.Select(i => new GradeComponentDto
        {
            ComponentName = i.ComponentName,
            Weight = i.Weight,
            MaxScore = i.MaxScore,
            OrderIndex = i.OrderIndex,
            Description = i.Description
        }).ToList();

        return await SaveGradeComponentsAsync(courseOfferingId, components);
    }

    #region Helper Methods

    private static (string LetterGrade, decimal GradePoint) GetLetterGrade(decimal percentage)
    {
        return percentage switch
        {
            >= 90 => ("A", 4.0m),
            >= 85 => ("B+", 3.3m),
            >= 80 => ("B", 3.0m),
            >= 75 => ("C+", 2.3m),
            >= 70 => ("C", 2.0m),
            >= 60 => ("D", 1.0m),
            _ => ("F", 0.0m)
        };
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
            "F" => 0.0m,
            _ => 0.0m
        };
    }

    #endregion
}
