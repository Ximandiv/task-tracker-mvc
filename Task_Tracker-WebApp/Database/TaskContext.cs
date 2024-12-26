using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Database;

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options) { }

    public required DbSet<User> Users { get; set; }
    public required DbSet<RememberMeToken> RememberMeTokens { get; set; }
    public required DbSet<UserTask> Tasks { get; set; }
}
