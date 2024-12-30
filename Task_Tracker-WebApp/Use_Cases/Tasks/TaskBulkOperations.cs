using System.Diagnostics;
using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Cache;

namespace Task_Tracker_WebApp.Use_Cases.Tasks
{
    public class TaskBulkOperations
        (IUnitOfWork unitOfWork,
        IMemoryCacheHandler cacheHandler,
        ILogger<TaskBulkOperations> logger)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMemoryCacheHandler _cacheHandler = cacheHandler;
        private readonly ILogger<TaskBulkOperations> _logger = logger;

        public async Task<bool> RemoveMany(List<int> idList, int userId)
        {
            try
            {
                var userTasks = await retrieveManyByUserAndId(idList, userId);

                if (isEntityListEmpty(userTasks))
                    return false;

                _cacheHandler.Remove(CachePrefix.UserTaskList, userId.ToString());

                await _unitOfWork.BeginTransaction();

                _unitOfWork.Tasks.RemoveMany(userTasks!);
                await _unitOfWork.SaveChanges();

                await _unitOfWork.CommitTransaction();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Remove Many bulk operation");
                throw;
            }
        }

        public async Task<bool> UpdateManyStatus
            (List<int> idList,
            int userId,
            string status)
        {
            try
            {
                var userTasks = await retrieveManyByUserAndId(idList, userId);

                if (isEntityListEmpty(userTasks))
                    return false;

                if (userTasks!.All(t => t.Status == status))
                    return false;

                _cacheHandler.Remove(CachePrefix.UserTaskList, userId.ToString());

                await _unitOfWork.BeginTransaction();

                _unitOfWork.Tasks.UpdateManyStatus(userTasks!, status);
                await _unitOfWork.SaveChanges();

                await _unitOfWork.CommitTransaction();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Remove Many bulk operation");
                throw;
            }
        }

        private async Task<IEnumerable<UserTask>?> retrieveManyByUserAndId
            (List<int> idList,
            int userId)
        {
            bool didCacheFindAll = false;
            if (_cacheHandler.Get(CachePrefix.UserTaskList,
                userId.ToString(),
                out IEnumerable<UserTask>? userTasks)
                && userTasks != null)
                didCacheFindAll = new HashSet<int>(idList).SetEquals(userTasks.Select(uT => uT.Id));

            if (!didCacheFindAll)
                return await _unitOfWork.Tasks.GetManyByIdAndUser(idList, userId);

            return userTasks;
        }

        private bool isEntityListEmpty(IEnumerable<UserTask>? userTasks)
            => userTasks == null || userTasks.Count() < 0;
    }
}
