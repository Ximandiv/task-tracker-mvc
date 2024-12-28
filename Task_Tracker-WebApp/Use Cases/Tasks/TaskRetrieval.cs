using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;
using Task_Tracker_WebApp.Use_Cases.Cache;
using Task_Tracker_WebApp.Repositories.Interfaces;

namespace Task_Tracker_WebApp.Use_Cases.Tasks
{
    public class TaskRetrieval
        (IUnitOfWork unitOfWork,
        IMemoryCacheHandler cacheHandler,
        ILogger<TaskRetrieval> logger)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMemoryCacheHandler _cacheHandler = cacheHandler;
        private readonly ILogger<TaskRetrieval> _logger = logger;

        public async Task<UserTaskListViewModel?> GetAllUserTasks
            (AuthorizedUser user,
            int pageNumber)
        {
            IEnumerable<UserTaskModel> taskResponseList;
            IEnumerable<UserTask>? taskList;
            try
            {
                if (!_cacheHandler.Get(CachePrefix.UserTaskList,
                                user.Id!.ToString()!,
                                out taskList))
                {
                    taskList = await _unitOfWork.Tasks.GetAllUserTasks(user.Id.Value);

                    _cacheHandler.Set(CachePrefix.UserTaskList,
                                    user.Id!.ToString()!,
                                    taskList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting User's tasks");
                throw;
            }

            if (taskList == null
                || taskList.Count() == 0)
                return null;

            taskResponseList = taskList!
                                    .Select(t => t.ToModel())
                                    .Skip((pageNumber - 1) * TaskLimitValidation.DashboardPageSize)
                                    .Take(TaskLimitValidation.DashboardPageSize)
                                    .ToList();

            int totalTasks = taskList!.Count();
            int totalPages = calculateTotalPages(totalTasks, TaskLimitValidation.DashboardPageSize);

            return new UserTaskListViewModel
                    (taskResponseList,
                    pageNumber,
                    TaskLimitValidation.DashboardPageSize,
                    totalPages,
                    totalTasks);
        }

        public async Task<UserTaskViewModel?> GetByIdAndUser
            (int taskId,
            int userId)
        {
            if (!_cacheHandler.Get(CachePrefix.UserTask, $"{userId}_{taskId}", out UserTask? userTask))
            {
                userTask = await _unitOfWork.Tasks.GetByIdAndUser(taskId, userId);

                if (userTask == null)
                    return null;

                _cacheHandler.Set(CachePrefix.UserTask, $"{userId}_{taskId}", userTask);
            }

            if (userTask == null) return null;

            return new UserTaskViewModel()
            {
                UserTask = new UserTaskModel(userTask)
            };
        }
        public async Task<bool> isTitleDuplicate
                    (AuthorizedUser user,
                    string title)
        {
            if (!_cacheHandler.Get(CachePrefix.UserTaskList,
                                user.Id!.ToString()!,
                                out IEnumerable<UserTask>? taskList))
                return await _unitOfWork.Tasks.IsTaskTitleDuplicate(user.Id!.Value, title);

            if (taskList == null) return false;

            return taskList!.Any(t => t.UserId == user.Id
                                && t.Title == title);
        }

        private int calculateTotalPages
            (int totalTasks,
            double dashboardPageSize)
            => (int)Math.Ceiling(totalTasks / dashboardPageSize);
    }
}
