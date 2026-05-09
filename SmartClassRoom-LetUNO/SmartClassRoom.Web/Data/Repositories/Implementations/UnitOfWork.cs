using Microsoft.EntityFrameworkCore.Storage;
using SmartClassRoom.Web.Data.Repositories.Interfaces;

namespace SmartClassRoom.Web.Data.Repositories.Implementations;

/// <summary>
/// Unit of Work pattern implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-initialized repositories
    private IStudentRepository? _students;
    private ITeacherRepository? _teachers;
    private ICourseRepository? _courses;
    private IDepartmentRepository? _departments;
    private ILevelRepository? _levels;
    private ISectionRepository? _sections;
    private IGroupRepository? _groups;
    private ITermRepository? _terms;
    private ICourseOfferingRepository? _courseOfferings;
    private IStudentEnrollmentRepository? _studentEnrollments;
    private ITimetableRepository? _timetables;
    private IScheduledSlotRepository? _scheduledSlots;
    private IRoomRepository? _rooms;
    private IAttendanceRepository? _attendances;
    private ISessionRepository? _sessions;
    private IQuizRepository? _quizzes;
    private IQuizQuestionRepository? _quizQuestions;
    private IQuizAssignmentRepository? _quizAssignments;
    private IQuizAttemptRepository? _quizAttempts;
    private IGradeRepository? _grades;
    private IGradeComponentRepository? _gradeComponents;
    private IGradeComponentTemplateRepository? _gradeComponentTemplates;
    private IMaterialRepository? _materials;
    private IMaterialFolderRepository? _materialFolders;
    private ITeacherAttendanceRepository? _teacherAttendances;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Repository Properties with Lazy Initialization

    public IStudentRepository Students
    {
        get
        {
            _students ??= new StudentRepository(_context);
            return _students;
        }
    }

    public ITeacherRepository Teachers
    {
        get
        {
            _teachers ??= new TeacherRepository(_context);
            return _teachers;
        }
    }

    public ICourseRepository Courses
    {
        get
        {
            _courses ??= new CourseRepository(_context);
            return _courses;
        }
    }

    public IDepartmentRepository Departments
    {
        get
        {
            _departments ??= new DepartmentRepository(_context);
            return _departments;
        }
    }

    public ILevelRepository Levels
    {
        get
        {
            _levels ??= new LevelRepository(_context);
            return _levels;
        }
    }

    public ISectionRepository Sections
    {
        get
        {
            _sections ??= new SectionRepository(_context);
            return _sections;
        }
    }

    public IGroupRepository Groups
    {
        get
        {
            _groups ??= new GroupRepository(_context);
            return _groups;
        }
    }

    public ITermRepository Terms
    {
        get
        {
            _terms ??= new TermRepository(_context);
            return _terms;
        }
    }

    public ICourseOfferingRepository CourseOfferings
    {
        get
        {
            _courseOfferings ??= new CourseOfferingRepository(_context);
            return _courseOfferings;
        }
    }

    public IStudentEnrollmentRepository StudentEnrollments
    {
        get
        {
            _studentEnrollments ??= new StudentEnrollmentRepository(_context);
            return _studentEnrollments;
        }
    }

    public ITimetableRepository Timetables
    {
        get
        {
            _timetables ??= new TimetableRepository(_context);
            return _timetables;
        }
    }

    public IScheduledSlotRepository ScheduledSlots
    {
        get
        {
            _scheduledSlots ??= new ScheduledSlotRepository(_context);
            return _scheduledSlots;
        }
    }

    public IRoomRepository Rooms
    {
        get
        {
            _rooms ??= new RoomRepository(_context);
            return _rooms;
        }
    }

    public IAttendanceRepository Attendances
    {
        get
        {
            _attendances ??= new AttendanceRepository(_context);
            return _attendances;
        }
    }

    public ISessionRepository Sessions
    {
        get
        {
            _sessions ??= new SessionRepository(_context);
            return _sessions;
        }
    }

    public IQuizRepository Quizzes
    {
        get
        {
            _quizzes ??= new QuizRepository(_context);
            return _quizzes;
        }
    }

    public IQuizQuestionRepository QuizQuestions
    {
        get
        {
            _quizQuestions ??= new QuizQuestionRepository(_context);
            return _quizQuestions;
        }
    }

    public IQuizAssignmentRepository QuizAssignments
    {
        get
        {
            _quizAssignments ??= new QuizAssignmentRepository(_context);
            return _quizAssignments;
        }
    }

    public IQuizAttemptRepository QuizAttempts
    {
        get
        {
            _quizAttempts ??= new QuizAttemptRepository(_context);
            return _quizAttempts;
        }
    }

    public IGradeRepository Grades
    {
        get
        {
            _grades ??= new GradeRepository(_context);
            return _grades;
        }
    }

    public IGradeComponentRepository GradeComponents
    {
        get
        {
            _gradeComponents ??= new GradeComponentRepository(_context);
            return _gradeComponents;
        }
    }

    public IGradeComponentTemplateRepository GradeComponentTemplates
    {
        get
        {
            _gradeComponentTemplates ??= new GradeComponentTemplateRepository(_context);
            return _gradeComponentTemplates;
        }
    }

    public IMaterialRepository Materials
    {
        get
        {
            _materials ??= new MaterialRepository(_context);
            return _materials;
        }
    }

    public IMaterialFolderRepository MaterialFolders
    {
        get
        {
            _materialFolders ??= new MaterialFolderRepository(_context);
            return _materialFolders;
        }
    }

    public ITeacherAttendanceRepository TeacherAttendances
    {
        get
        {
            _teacherAttendances ??= new TeacherAttendanceRepository(_context);
            return _teacherAttendances;
        }
    }

    /// <summary>
    /// Get generic repository for any entity type (for lookup tables)
    /// </summary>
    public IRepository<T> Repository<T>() where T : class
    {
        return new Repository<T>(_context);
    }

    // Transaction Management

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitAsync()
    {
        try
        {
            await SaveChangesAsync();

            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Dispose Pattern

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
