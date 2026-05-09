using Microsoft.EntityFrameworkCore.Storage;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions and repository coordination
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository Properties - Lazy initialized
    IStudentRepository Students { get; }
    ITeacherRepository Teachers { get; }
    ICourseRepository Courses { get; }
    IDepartmentRepository Departments { get; }
    ILevelRepository Levels { get; }
    ISectionRepository Sections { get; }
    IGroupRepository Groups { get; }
    ITermRepository Terms { get; }
    ICourseOfferingRepository CourseOfferings { get; }
    IStudentEnrollmentRepository StudentEnrollments { get; }
    ITimetableRepository Timetables { get; }
    IScheduledSlotRepository ScheduledSlots { get; }
    IRoomRepository Rooms { get; }
    IAttendanceRepository Attendances { get; }
    ISessionRepository Sessions { get; }
    IQuizRepository Quizzes { get; }
    IQuizQuestionRepository QuizQuestions { get; }
    IQuizAssignmentRepository QuizAssignments { get; }
    IQuizAttemptRepository QuizAttempts { get; }
    IGradeRepository Grades { get; }
    IGradeComponentRepository GradeComponents { get; }
    IGradeComponentTemplateRepository GradeComponentTemplates { get; }
    IMaterialRepository Materials { get; }
    IMaterialFolderRepository MaterialFolders { get; }
    ITeacherAttendanceRepository TeacherAttendances { get; }

    /// <summary>
    /// Get generic repository for any entity type
    /// </summary>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    /// <returns>Number of affected rows</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Save changes with cancellation token support
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begin a database transaction
    /// </summary>
    Task<IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackAsync();
}
