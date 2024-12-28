using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmail(string email);
    }
}
