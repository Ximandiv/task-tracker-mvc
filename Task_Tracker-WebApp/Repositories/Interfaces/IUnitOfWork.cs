using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRememberTokenRepository RememberTokens { get; }
        ITaskRepository Tasks { get; }
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task SaveChanges();
    }
}
