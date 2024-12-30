using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;
using Task_Tracker_WebApp.Use_Cases.Cache;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Tasks.Validators;
using System.Threading.Tasks;

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
        private readonly int pageOffset = 1;

        public async Task<UserTaskListViewModel?> GetUserTasksByFilter
            (AuthorizedUser user,
            int pageNumber,
            int filterNumber)
        {
            if (!TaskStatusValidator.ValidStatus(filterNumber, out string statusFilter))
                return await GetAllUserTasks(user, pageNumber);

            IEnumerable<UserTask>? taskList = await retrieveUserTasks(user.Id!.Value);

            IEnumerable<UserTaskModel> taskResponseList
                = taskList!
                    .Where(t => t.Status == statusFilter)
                    .Select(t => t.ToModel())
                    .ToList();
            int totalTasks = taskResponseList.Count();

            taskResponseList = taskResponseList
                .Skip((pageNumber - pageOffset) * TaskLimitValidation.DashboardPageSize)
                .Take(TaskLimitValidation.DashboardPageSize)
                .ToList();

            return buildListViewModel(taskResponseList, pageNumber, totalTasks);
        }

        public async Task<UserTaskListViewModel?> GetAllUserTasks
            (AuthorizedUser user,
            int pageNumber)
        {
            IEnumerable<UserTask>? taskList = await retrieveUserTasks(user.Id!.Value);

            IEnumerable<UserTaskModel> taskResponseList
                = taskList!
                    .Select(t => t.ToModel())
                    .ToList();
            int totalTasks = taskResponseList.Count();

            taskResponseList = taskResponseList
                .Skip((pageNumber - pageOffset) * TaskLimitValidation.DashboardPageSize)
                .Take(TaskLimitValidation.DashboardPageSize)
                .ToList();

            return buildListViewModel(taskResponseList, pageNumber, totalTasks);
        }

        public async Task<UserTaskViewModel?> GetByIdAndUser
            (int taskId,
            int userId)
        {
            UserTask? userTask = await getEntityByIdAndUser(taskId, userId);

            if (userTask == null) return null;

            return new UserTaskViewModel()
            {
                UserTask = new UserTaskModel(userTask)
            };
        }

        public async Task<bool> IsTitleDuplicate
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

        private UserTaskListViewModel buildListViewModel
            (IEnumerable<UserTaskModel> taskResponseList,
            int pageNumber,
            int totalTasks)
            => new UserTaskListViewModel
                    (taskResponseList,
                    pageNumber,
                    TaskLimitValidation.DashboardPageSize,
                    calculateTotalPages(totalTasks, TaskLimitValidation.DashboardPageSize),
                    totalTasks);

        private async Task<IEnumerable<UserTask>?> retrieveUserTasks
            (int userId)
        {
            IEnumerable<UserTask>? taskList;
            try
            {
                if (!_cacheHandler.Get(CachePrefix.UserTaskList,
                                userId.ToString(),
                                out taskList))
                {
                    taskList = await _unitOfWork.Tasks.GetAllUserTasks(userId);

                    _cacheHandler.Set(CachePrefix.UserTaskList,
                                    userId.ToString(),
                                    taskList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting User's tasks");
                throw;
            }

            return taskList;
        }

        private async Task<UserTask?> getEntityByIdAndUser(int taskId, int userId)
        {
            if (!_cacheHandler.Get(CachePrefix.UserTask, $"{userId}_{taskId}", out UserTask? userTask))
            {
                userTask = await _unitOfWork.Tasks.GetByIdAndUser(taskId, userId);

                if (userTask == null)
                    return null;

                _cacheHandler.Set(CachePrefix.UserTask, $"{userId}_{taskId}", userTask);
            }

            return userTask;
        }
    }
}
