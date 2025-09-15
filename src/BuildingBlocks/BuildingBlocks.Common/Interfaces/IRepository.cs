using System.Linq.Expressions;

namespace BuildingBlocks.Common.Interfaces
{
    /// <summary>
    /// Generic repository pattern interface for data access operations
    /// Provides CRUD operations with async support and expression-based querying
    /// </summary>
    /// <typeparam name="TEntity">The entity type managed by this repository</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key</typeparam>
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        #region Query Operations

        /// <summary>
        /// Retrieves an entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities from the repository
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of all entities</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds entities based on a predicate expression
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of matching entities</returns>
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entity matching the predicate or null
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First matching entity or null</returns>
        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Single matching entity</returns>
        /// <exception cref="InvalidOperationException">When zero or multiple entities match</exception>
        Task<TEntity> SingleAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the predicate or null
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Single matching entity or null</returns>
        /// <exception cref="InvalidOperationException">When multiple entities match</exception>
        Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the predicate
        /// </summary>
        /// <param name="predicate">Filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if any entity matches, false otherwise</returns>
        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities matching the predicate
        /// </summary>
        /// <param name="predicate">Optional filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of matching entities</returns>
        Task<int> CountAsync(
            Expression<Func<TEntity, bool>>? predicate = null, 
            CancellationToken cancellationToken = default);

        #endregion

        #region Command Operations

        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added entity</returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds multiple entities to the repository
        /// </summary>
        /// <param name="entities">Entities to add</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The added entities</returns>
        Task<IEnumerable<TEntity>> AddRangeAsync(
            IEnumerable<TEntity> entities, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task UpdateRangeAsync(
            IEnumerable<TEntity> entities, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity by its identifier
        /// </summary>
        /// <param name="id">Entity identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity from the repository
        /// </summary>
        /// <param name="entity">Entity to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes multiple entities from the repository
        /// </summary>
        /// <param name="entities">Entities to remove</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task DeleteRangeAsync(
            IEnumerable<TEntity> entities, 
            CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Simplified repository interface for entities with Guid keys
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
    {
    }

    /// <summary>
    /// Unit of Work pattern interface for coordinating multiple repository operations
    /// Ensures data consistency across multiple operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets a repository instance for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Repository instance</returns>
        IRepository<TEntity> Repository<TEntity>() where TEntity : class;

        /// <summary>
        /// Gets a repository instance for the specified entity type with custom key type
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <returns>Repository instance</returns>
        IRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class;

        /// <summary>
        /// Saves all pending changes to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of affected records</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new database transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current transaction
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indicates whether there's an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}