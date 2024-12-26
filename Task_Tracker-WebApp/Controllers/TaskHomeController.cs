using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Task_Tracker_WebApp.Cache;
using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models;
using Task_Tracker_WebApp.Models.View;

namespace Task_Tracker_WebApp.Controllers
{
    [Authorize]
    public class TaskHomeController : Controller
    {
        private readonly ILogger<TaskHomeController> _logger;
        private readonly MemoryCacheHandler _cache;
        private readonly TaskContext _taskContext;

        private readonly int dashboardPageSize = 6;

        public TaskHomeController(TaskContext context,
            ILogger<TaskHomeController> logger,
            MemoryCacheHandler cache)
        {
            _taskContext = context;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(int pageNumber = 1)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("username");

            if (!int.TryParse(userIdString, out int userId)
                || string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                ViewData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            IEnumerable<UserTaskViewModel> taskResponseList;
            if (!_cache.Get(CachePrefix.UserTaskList, 
                            userId.ToString(), 
                            out IEnumerable<UserTask>? taskList))
            {
                taskList = await _taskContext.Tasks
                                .AsNoTracking()
                                .OrderBy(t => t.Id)
                                .Where(t => t.UserId == userId)
                                .ToListAsync();

                _cache.Set(CachePrefix.UserTaskList,
                            userId.ToString(),
                            taskList);
            }

            taskResponseList = taskList!
                                    .Select(t => new UserTaskViewModel()
                                        {
                                            UserTask = new UserTaskResponse()
                                            {
                                                Id = t.Id,
                                                Title = t.Title,
                                                Description = t.Description,
                                                Status = t.Status,
                                                CreatedDate = t.CreatedDate,
                                                UpdatedDate = t.UpdatedDate
                                            }
                                        }).ToList();

            var paginatedResult = taskResponseList!
                                .Skip((pageNumber - 1) * dashboardPageSize)
                                .Take(dashboardPageSize)
                                .ToList();

            int totalTasks = taskResponseList!.Count();
            int totalPages = (int)Math.Ceiling(totalTasks / (double)dashboardPageSize);
            var viewModelResult = new UserTaskListViewModel()
            {
                Tasks = paginatedResult,
                PageNumber = pageNumber,
                PageSize = dashboardPageSize,
                TotalPages = totalPages,
                TotalTasks = totalTasks
            };

            ViewData["Username"] = userName;
            ViewData["ActualPage"] = pageNumber;
            ViewData["SuccessOperation"] = TempData["SuccessOperation"] ?? null;
            ViewData["DeleteError"] = TempData["DeleteError"] ?? null;

            return View(viewModelResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("username");

            if (!int.TryParse(userIdString, out int userId)
                || string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Username"] = userName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserTaskViewModel taskViewModel)
        {
            if (!ModelState.IsValid)
                return View(taskViewModel);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (!_cache.Get(CachePrefix.UserTaskList,
                        userId.ToString(),
                        out IEnumerable<UserTask>? taskList))
            {
                taskList = await _taskContext.Tasks
                                                .AsNoTracking()
                                                .OrderBy(t => t.Id)
                                                .Where(t => t.UserId == userId)
                                                .ToListAsync();

                _cache.Set(CachePrefix.UserTaskList, userId.ToString(), taskList);
            }
            
            bool isTaskTitleDuplicate = taskList!.Any(t => t.UserId == userId 
                                                    && t.Title == taskViewModel.UserTask!.Title);

            if (isTaskTitleDuplicate)
            {
                ViewData["GeneralError"] = "A Task with the same Title was found";
                return View(taskViewModel);
            }

            var userTask = await add(taskViewModel.UserTask!, userId);

            _cache.Set(CachePrefix.UserTask, $"{userId}_{userTask.Id}", userTask);
            _cache.Remove(CachePrefix.UserTaskList, $"{userId}");

            TempData["SuccessOperation"] = "Task was successfully created";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int taskId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("username");

            if (!int.TryParse(userIdString, out int userId)
                || string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (!_cache.Get(CachePrefix.UserTask, $"{userId}_{taskId}", out UserTask? userTask))
            {
                userTask = await _taskContext.Tasks
                                                .FirstOrDefaultAsync(t =>
                                                                    t.Id == taskId
                                                                    && t.UserId == userId);

                if (userTask == null)
                {
                    TempData["DeleteError"] = "Task to edit was not found";
                    return RedirectToAction("Dashboard");
                }

                _cache.Set(CachePrefix.UserTask, $"{userId}_{taskId}", userTask);
            }

            UserTaskViewModel response = new()
            {
                UserTask = new UserTaskResponse()
                {
                    Id = userTask!.Id,
                    Title = userTask!.Title,
                    Description = userTask!.Description,
                    Status = userTask!.Status,
                    CreatedDate = userTask!.CreatedDate,
                    UpdatedDate = userTask!.UpdatedDate,
                }
            };

            ViewData["Username"] = userName;

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (!_cache.Get(CachePrefix.UserTask, $"{userId}_{model.UserTask!.Id}", out UserTask? userTask))
            {
                userTask = await _taskContext.Tasks
                                                .FirstOrDefaultAsync(t =>
                                                                    t.Id == model.UserTask!.Id
                                                                    && t.UserId == userId);

                if (userTask == null)
                {
                    TempData["DeleteError"] = "Task to delete was not found";
                    return RedirectToAction("Dashboard");
                }

                _cache.Set(CachePrefix.UserTask, $"{userId}_{model.UserTask!.Id}", userTask);
            }

            if(model.UserTask.Title == userTask!.Title
                && model.UserTask.Description == userTask!.Description
                && model.UserTask.Status == userTask!.Status)
            {
                ViewData["GeneralError"] = "Task was unchanged";
                return View(model);
            }

            using (var transaction = await _taskContext.Database.BeginTransactionAsync())
            {
                try
                {
                    userTask!.Title = model.UserTask.Title;
                    userTask!.Description = model.UserTask.Description;
                    userTask!.Status = model.UserTask.Status;
                    userTask!.UpdatedDate = DateTime.UtcNow;

                    _taskContext.Tasks.Update(userTask);
                    await _taskContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($@"An error occurred during a transaction in Task Edition
                                    where id is {model.UserTask.Id} and exception {ex.Message}");
                }
            }

            _cache.Remove(CachePrefix.UserTaskList, userId.ToString());
            _cache.Remove(CachePrefix.UserTask, $"{userId}_{model.UserTask.Id}");

            TempData["SuccessOperation"] = "Task was successfully edited";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int taskId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (!_cache.Get(CachePrefix.UserTask, $"{userId}_{taskId}", out UserTask? userTask))
            {
                userTask = await _taskContext.Tasks
                                                .FirstOrDefaultAsync(t =>
                                                                    t.Id == taskId
                                                                    && t.UserId == userId);

                if (userTask == null)
                {
                    TempData["DeleteError"] = "Task to delete was not found";
                    return View("Dashboard");
                }
            }

            using (var transaction = await _taskContext.Database.BeginTransactionAsync())
            {
                try
                {
                    _taskContext.Tasks.Remove(userTask!);
                    await _taskContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($@"An error occurred during a transaction in Task Deletion
                                    where id is {taskId} and exception {ex.Message}");
                }
            }

            _cache.Remove(CachePrefix.UserTaskList, userId.ToString());
            _cache.Remove(CachePrefix.UserTask, $"{userId}_{taskId}");

            TempData["SuccessOperation"] = "Task was successfully removed";
            return RedirectToAction("Dashboard");
        }

        private async Task<UserTask> add(UserTaskResponse taskViewModel, int userId)
        {
            UserTask userTask = new()
            {
                Title = taskViewModel.Title,
                Description = taskViewModel.Description,
                Status = taskViewModel.Status,
                UserId = userId
            };

            using (var transaction = await _taskContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _taskContext.Tasks.AddAsync(userTask);
                    await _taskContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($@"An error occurred during a transaction in Task Creation
                                    where exception {ex.Message}");
                }
            }

            return userTask;
        }
    }
}
