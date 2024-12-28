using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Use_Cases.Cache;

namespace Task_Tracker_WebApp.Use_Cases.Tasks
{
    public class TaskOperations
        (IUnitOfWork unitOfWork,
        IMemoryCacheHandler cacheHandler,
        ILogger<TaskOperations> logger)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMemoryCacheHandler _cacheHandler = cacheHandler;
        private readonly ILogger<TaskOperations> _logger = logger;

        public async Task Create
            (AuthorizedUser authUser,
            UserTaskModel userTaskModel
            )
        {
            UserTask userTask = new(userTaskModel, authUser.Id!.Value);
            try
            {
                await _unitOfWork.BeginTransaction();
                await _unitOfWork.Tasks.Add(userTask);
                await _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $@"An error occurred during a transaction in Task Creation");
                throw;
            }

            _cacheHandler.Set(CachePrefix.UserTask, $"{authUser.Id}_{userTask.Id}", userTask);
            _cacheHandler.Remove(CachePrefix.UserTaskList, $"{authUser.Id}");
        }

        public async Task<string> Update
            (AuthorizedUser user,
            UserTaskModel userTaskModel
            )
        {
            if (!_cacheHandler.Get
                (CachePrefix.UserTask, 
                $"{user.Id!.Value}_{userTaskModel.Id}", 
                out UserTask? userTask))
            {
                userTask = await _unitOfWork.Tasks
                                            .GetByIdAndUser(userTaskModel.Id!.Value, user.Id!.Value);

                if (userTask == null) return "Task not found";

                _cacheHandler.Set(CachePrefix.UserTask, $"{user.Id!.Value}_{userTaskModel.Id}", userTask);
            }

            if (userTaskModel.Title == userTask!.Title
                && userTaskModel.Description == userTask!.Description
                && userTaskModel.Status == userTask!.Status)
                return "Task was unchanged";

            try
            {
                userTask.Update(userTaskModel);

                await _unitOfWork.BeginTransaction();
                _unitOfWork.Tasks.Update(userTask);
                await _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $@"An error occurred during a transaction in Task Edition
                                where id is {userTask.Id}");
                throw;
            }

            _cacheHandler.Remove(CachePrefix.UserTaskList, user.Id!.ToString()!);
            _cacheHandler.Remove(CachePrefix.UserTask, $"{user.Id!}_{userTask.Id}");

            return string.Empty;
        }

        public async Task<string> Remove
            (AuthorizedUser user,
            int taskId)
        {
            if (!_cacheHandler.Get(CachePrefix.UserTask, $"{user.Id!.Value}_{taskId}", out UserTask? userTask))
            {
                userTask = await _unitOfWork.Tasks.GetByIdAndUser(taskId, user.Id!.Value);

                if (userTask == null)
                    return "Task to delete was not found";
            }

            try
            {
                await _unitOfWork.BeginTransaction();
                _unitOfWork.Tasks.Delete(userTask!);
                await _unitOfWork.SaveChanges();
                await _unitOfWork.CommitTransaction();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $@"An error occurred during a transaction in Task Deletion
                                    where id is {taskId}");
                return "An error occurred during Task deletion";
            }

            _cacheHandler.Remove(CachePrefix.UserTaskList, user.Id!.ToString()!);
            _cacheHandler.Remove(CachePrefix.UserTask, $"{user.Id!}_{taskId}");

            return string.Empty;
        }
    }
}
