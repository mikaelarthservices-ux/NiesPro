using Auth.Application.Contracts.Services;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Auth.Infrastructure.Services
{
    /// <summary>
    /// Unit of Work implementation for Auth.Application
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        public UnitOfWork(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await _transaction.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to rollback");

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _disposed = true;
            }
        }
    }
}