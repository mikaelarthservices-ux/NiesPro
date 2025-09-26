namespace Customer.Domain.Interfaces
{
    /// <summary>
    /// Customer repository interface
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Get customer by ID
        /// </summary>
        Task<Aggregates.CustomerAggregate.Customer?> GetByIdAsync(Guid id);

        /// <summary>
        /// Get customer by email
        /// </summary>
        Task<Aggregates.CustomerAggregate.Customer?> GetByEmailAsync(string email);

        /// <summary>
        /// Get customers with pagination
        /// </summary>
        Task<(List<Aggregates.CustomerAggregate.Customer> customers, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);

        /// <summary>
        /// Add new customer
        /// </summary>
        Task AddAsync(Aggregates.CustomerAggregate.Customer customer);

        /// <summary>
        /// Update existing customer
        /// </summary>
        Task UpdateAsync(Aggregates.CustomerAggregate.Customer customer);

        /// <summary>
        /// Delete customer
        /// </summary>
        Task DeleteAsync(Aggregates.CustomerAggregate.Customer customer);

        /// <summary>
        /// Check if customer exists by email
        /// </summary>
        Task<bool> ExistsAsync(string email);
    }

    /// <summary>
    /// Unit of Work pattern interface
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Save all changes
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}