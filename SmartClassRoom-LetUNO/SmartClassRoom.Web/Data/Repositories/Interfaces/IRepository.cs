using System.Linq.Expressions;

namespace SmartClassRoom.Web.Data.Repositories.Interfaces;

/// <summary>
/// Generic repository interface with common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    // Read Operations

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Get entity by ID with navigation properties included
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="includeProperties">Navigation properties to include (e.g., "Department", "Level")</param>
    Task<T?> GetByIdAsync(int id, params string[] includeProperties);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Get all entities with navigation properties included
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(params string[] includeProperties);

    /// <summary>
    /// Find entities matching a predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Find entities matching a predicate with navigation properties included
    /// </summary>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        params string[] includeProperties);

    /// <summary>
    /// Get paginated results
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="orderBy">Order by expression</param>
    /// <param name="ascending">Sort direction</param>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    /// <summary>
    /// Get paginated results with filter
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<T, bool>> predicate,
        int pageNumber,
        int pageSize,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    /// <summary>
    /// Check if any entity matches the predicate
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Count entities matching the predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Get first entity matching predicate or null
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Get first entity matching predicate with includes or null
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params string[] includeProperties);

    // Write Operations

    /// <summary>
    /// Add a new entity
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Update an existing entity
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Update multiple entities
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// Delete an entity
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);
}
