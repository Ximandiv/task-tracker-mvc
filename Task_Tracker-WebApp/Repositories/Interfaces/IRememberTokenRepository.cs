using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Repositories.Interfaces
{
    public interface IRememberTokenRepository : IRepository<RememberMeToken>
    {
        Task<RememberMeToken?> GetByToken(string token);
    }
}
