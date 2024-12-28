using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Repositories.Instances
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly TaskContext _taskContext;
        public UserRepository(TaskContext context)
            : base(context)
        {
            _taskContext = context;
        }

        public async Task<User?> GetByEmail(string email)
            => await _taskContext.Users
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.Email == email);
    }
}
