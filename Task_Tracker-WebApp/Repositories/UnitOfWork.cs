using Microsoft.EntityFrameworkCore.Storage;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Repositories.Instances;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Repositories
{
    public class UnitOfWork
        (TaskContext context,
        ILogger<UnitOfWork> logger)
        : IUnitOfWork
    {
        private readonly ILogger<UnitOfWork> _logger = logger;

        private readonly TaskContext _context = context;
        private IDbContextTransaction? _currentTransaction;

        public IUserRepository Users { get; } = new UserRepository(context);
        public IRememberTokenRepository RememberTokens { get; } = new RememberTokenRepository(context);
        public ITaskRepository Tasks { get; } = new TaskRepository(context);

        public async Task BeginTransaction()
        {
            if (_currentTransaction != null) return;

            try
            {
                _currentTransaction = await _context.Database.BeginTransactionAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error starting a transaction");
            }
        }

        public async Task CommitTransaction()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error committing the transaction. Rolling back...");
                await RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransaction()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error during transaction rollback");
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task SaveChanges() => await _context.SaveChangesAsync();

        public void Dispose() { }
    }
}
