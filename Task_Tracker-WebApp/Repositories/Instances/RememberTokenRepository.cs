using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Repositories.Instances
{
    public class RememberTokenRepository : Repository<RememberMeToken>, IRememberTokenRepository
    {
        private readonly TaskContext _taskContext;
        public RememberTokenRepository(TaskContext context)
            : base(context)
        {
            _taskContext = context;
        }

        public async Task<RememberMeToken?> GetByToken(string token)
            => await _taskContext.RememberMeTokens
                                    .Include(t => t.User)
                                    .FirstOrDefaultAsync(t => t.Token == token);
    }
}
