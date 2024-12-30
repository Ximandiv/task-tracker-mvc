using Task_Tracker_WebApp.Database.Entities;

namespace Task_Tracker_WebApp.Repositories.Interfaces
{
    public interface ITaskRepository : IRepository<UserTask>
    {
        Task<IEnumerable<UserTask>?> GetAllUserTasks(int userId);
        Task<bool> IsTaskTitleDuplicate(int userId, string title);
        Task<UserTask?> GetByIdAndUser(int id, int userId);
        void UpdateManyStatus(IEnumerable<UserTask> taskList, string status);
        void RemoveMany(IEnumerable<UserTask> taskList);
        Task<IEnumerable<UserTask>?> GetManyByIdAndUser(IEnumerable<int> idList, int userId);
    }
}
