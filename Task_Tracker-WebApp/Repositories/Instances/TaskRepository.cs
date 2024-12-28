using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Repositories.Instances
{
    public class TaskRepository : Repository<UserTask>, ITaskRepository
    {
        private readonly TaskContext _context;
        public TaskRepository(TaskContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserTask>?> GetAllUserTasks(int userId)
            => await _context.Tasks
                        .AsNoTracking()
                        .OrderBy(t => t.Id)
                        .Where(t => t.UserId == userId)
                        .ToListAsync();

        public async Task<bool> IsTaskTitleDuplicate(int userId, string title)
            => await _context.Tasks
                    .AsNoTracking()
                    .AnyAsync(t => t.UserId == userId &&  t.Title == title);

        public async Task<UserTask?> GetByIdAndUser(int id, int userId)
            => await _context.Tasks
                                .AsNoTracking()
                                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }
}
