using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;
using Task_Tracker_WebApp.Use_Cases.Cache;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Tasks.Validators;

namespace Task_Tracker_WebApp.Use_Cases.Tasks
{
    public class TaskRetrieval
        (IUnitOfWork unitOfWork,
        ICacheHandler cacheHandler,
        ILogger<TaskRetrieval> logger)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICacheHandler _cacheHandler = cacheHandler;
        private readonly ILogger<TaskRetrieval> _logger = logger;
        private readonly int pageOffset = 1;

        public async Task<UserTaskListViewModel?> GetUserTasksByFilter
            (AuthorizedUser user,
            int pageNumber,
            int filterNumber)
        {
            if (!TaskStatusValidator.ValidStatus(filterNumber, out string statusFilter))
                return await getAllUserTasks(user, pageNumber);

            IEnumerable<UserTask>? taskList = await retrieveUserTasks(user.Id!.Value);

            IEnumerable<UserTaskModel> taskResponseList
                = taskList!
                    .Where(t => t.Status == statusFilter)
                    .Select(t => t.ToModel())
                    .ToList();
            int totalTasks = taskResponseList.Count();

            taskResponseList = paginateDashboardList(taskResponseList, pageNumber);

            if (!taskResponseList.Any())
            {
                pageNumber = 1;
                taskResponseList = paginateDashboardList(taskResponseList, pageNumber);
            }

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
            var taskList = await _cacheHandler.GetList<UserTask>
                            (CachePrefix.UserTaskList,
                                user.Id!.ToString()!);
            if (taskList is null)
                return await _unitOfWork.Tasks.IsTaskTitleDuplicate(user.Id!.Value, title);

            return taskList!.Any(t => t.UserId == user.Id
                                && t.Title == title);
        }

        private async Task<UserTaskListViewModel?> getAllUserTasks
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
            try
            {
                var taskList = await _cacheHandler.GetList<UserTask>
                                                        (CachePrefix.UserTaskList,
                                                        userId.ToString())
                                ?? await _unitOfWork.Tasks.GetAllUserTasks(userId);
                if (taskList is not null)
                    await _cacheHandler.SetList(CachePrefix.UserTaskList,
                                    userId.ToString(),
                                    taskList!);

                return taskList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting User's tasks");
                throw;
            }
        }

        private async Task<UserTask?> getEntityByIdAndUser(int taskId, int userId)
        {
            var userTask = await _cacheHandler.Get<UserTask>(CachePrefix.UserTask, $"{userId}_{taskId}")
                            ?? await _unitOfWork.Tasks.GetByIdAndUser(taskId, userId);
            if (userTask is not null)
                await _cacheHandler.Set(CachePrefix.UserTask, $"{userId}_{taskId}", userTask);

            return userTask;
        }

        private IEnumerable<UserTaskModel> paginateDashboardList
            (IEnumerable<UserTaskModel> list,
            int pageNumber)
            => list
                .Skip((pageNumber - pageOffset) * TaskLimitValidation.DashboardPageSize)
                .Take(TaskLimitValidation.DashboardPageSize)
                .ToList();
    }
}
