using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task_Tracker_WebApp.Cache;
using Task_Tracker_WebApp.Cache.Enums;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models.View;

namespace Task_Tracker_WebApp.Controllers
{
    [Authorize]
    public class TaskHomeController : Controller
    {
        private readonly ILogger<TaskHomeController> _logger;
        private readonly MemoryCacheHandler _cache;
        private readonly TaskContext _taskContext;

        public TaskHomeController(TaskContext context,
            ILogger<TaskHomeController> logger,
            MemoryCacheHandler cache)
        {
            _taskContext = context;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Dashboard()
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

            if (!_cache.Get(CachePrefix.UserTasks, userId, out IEnumerable<TaskViewModel>? tasksViewModel))
            {
                var tasks = _taskContext.Tasks.AsNoTracking().Where(t => t.UserId == userId).ToList();

                tasksViewModel = tasks.Select(t => new TaskViewModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate
                }).ToList();

                _cache.Set(CachePrefix.UserTasks, userId, tasksViewModel);
            }

            ViewData["Username"] = userName;

            return View(tasksViewModel);
        }

        public async Task<IActionResult> CreateOrEdit(int? id)
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

            ViewData["Username"] = userName;

            if (id is not null)
            {
                UserTask? foundTask;
                if (_cache.Get(CachePrefix.UserTasks, userId, out IEnumerable<UserTask>? userTasks))
                    foundTask = userTasks!.FirstOrDefault(t => t.Id == id);
                else
                    foundTask = await _taskContext.Tasks.FindAsync(id);

                if (foundTask is null)
                    return NotFound();

                var viewModel = new TaskViewModel()
                {
                    Id = foundTask.Id,
                    Title = foundTask.Title,
                    Description = foundTask.Description,
                    Status = foundTask.Status,
                    CreatedDate = foundTask.CreatedDate,
                    UpdatedDate = foundTask.UpdatedDate
                };
                return View(viewModel);
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveTask(TaskViewModel taskViewModel)
        {
            if (!ModelState.IsValid)
                return View("CreateOrEdit", taskViewModel);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                ViewData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (taskViewModel.Id == null)
                await add(taskViewModel, userId);
            else
            {
                UserTask? userTask = await _taskContext.Tasks.FindAsync(taskViewModel.Id);

                if (userTask is null)
                    NotFound();

                if (userTask is not null
                    && userTask.UserId != userId)
                {
                    ViewData["InvalidModel"] = "Task being edited is not yours.";
                    return View(taskViewModel);
                }

                await edit(userTask!, taskViewModel);
            }

            _cache.Remove(CachePrefix.UserTasks, userId);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdString, out int userId))
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values:
                        UserId: {userId}");

                ViewData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            var task = await _taskContext.Tasks.FindAsync(id);

            if (task is null)
                return NotFound();

            using (var transaction = await _taskContext.Database.BeginTransactionAsync())
            {
                try
                {
                    _taskContext.Tasks.Remove(task);
                    await _taskContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($@"An error occurred during a transaction in Task Deletion
                                    where id is {id} and exception {ex.Message}");
                }
            }

            _cache.Remove(CachePrefix.UserTasks, userId);

            return RedirectToAction("Dashboard");
        }

        private async Task add(TaskViewModel taskViewModel, int userId)
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
        }

        private async Task edit(
            UserTask userTask,
            TaskViewModel taskViewModel)
        {
            using (var transaction = await _taskContext.Database.BeginTransactionAsync())
            {
                try
                {
                    userTask.Title = taskViewModel.Title;
                    userTask.Description = taskViewModel.Description;
                    userTask.Status = taskViewModel.Status;
                    userTask.UpdatedDate = DateTime.Now;

                    _taskContext.Tasks.Update(userTask);
                    await _taskContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($@"An error occurred during a transaction in Task Edition
                                    where Task Id {userTask.Id} exception {ex.Message}");
                }
            }
        }
    }
}
