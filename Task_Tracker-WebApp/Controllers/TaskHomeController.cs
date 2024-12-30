using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Use_Cases.Auth;
using Task_Tracker_WebApp.Use_Cases.Tasks;

namespace Task_Tracker_WebApp.Controllers
{
    [Authorize]
    public class TaskHomeController
        (CredentialsHandler authService,
        TaskOperations taskOps,
        TaskBulkOperations taskBulkOps,
        TaskRetrieval taskRetrieval,
        ILogger<TaskHomeController> logger) : Controller
    {
        private readonly ILogger<TaskHomeController> _logger = logger;
        private readonly CredentialsHandler _authService = authService;
        private readonly TaskOperations _taskOperations = taskOps;
        private readonly TaskBulkOperations _taskBulkOperations = taskBulkOps;
        private readonly TaskRetrieval _taskRetrieval = taskRetrieval;

        [HttpGet]
        public async Task<IActionResult> Dashboard
            (int pageNumber = 1,
            int filterNumber = 0)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            UserTaskListViewModel? viewModelResult;
            try
            {
                if (filterNumber == 0)
                    viewModelResult = await _taskRetrieval.GetAllUserTasks(authUser, pageNumber);
                else
                {
                    pageNumber = 1;
                    viewModelResult = await _taskRetrieval.GetUserTasksByFilter(authUser, pageNumber, filterNumber);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GET endpoint .../TaskHome/Dashboard");

                ViewData["AuthError"] = "An error occurred during task retrieval";
                return RedirectToAction("Index", "Home");
            }

            TempData["ActualPage"] = pageNumber;

            ViewData["Username"] = authUser.UserName;
            ViewData["SuccessOperation"] = TempData["SuccessOperation"] ?? null;
            ViewData["DeleteError"] = TempData["DeleteError"] ?? null;

            return View(viewModelResult);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Username"] = authUser.UserName;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserTaskViewModel taskViewModel)
        {
            if (!ModelState.IsValid)
                return View(taskViewModel);

            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (await _taskRetrieval.IsTitleDuplicate(authUser, taskViewModel.UserTask!.Title))
            {
                ViewData["GeneralError"] = "A Task with the same Title was found";
                return View(taskViewModel);
            }

            try
            {
                await _taskOperations.Create(authUser, taskViewModel.UserTask!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $@"An error occurred in endpoint CREATE ../TaskHome/Create");

                ViewData["GeneralError"] = "An error occurred during Task Creation";
                return View(taskViewModel);
            }

            TempData["SuccessOperation"] = "Task was successfully created";
            return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int taskId)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = await _taskRetrieval.GetByIdAndUser(taskId, authUser.Id!.Value);

            ViewData["Username"] = authUser.UserName;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserTaskViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            string error = string.Empty;
            try
            {
                error = await _taskOperations.Update(authUser, model.UserTask!);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../TaskHome/Edit");
                ViewData["GeneralError"] = "An error occurred during task edition";

                return View(model);
            }

            if(!string.IsNullOrEmpty(error))
            {
                ViewData["GeneralError"] = error;
                return View(model);
            }

            TempData["SuccessOperation"] = "Task was successfully edited";
            return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int taskId)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            string error = string.Empty;
            try
            {
                error = await _taskOperations.Remove(authUser, taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../TaskHome/Edit");
                TempData["GeneralError"] = "An error occurred during task deletion";

                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            if(!string.IsNullOrEmpty(error))
            {
                TempData["DeleteError"] = error;
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            TempData["SuccessOperation"] = "Task was successfully removed";
            return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMany(string taskIdList)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(taskIdList))
            {
                TempData["DeleteError"] = "Selected Tasks to remove are invalid";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            var taskIdListParsed = taskIdList.Split(',')
                                            .Select(id => {
                                                int parsedId;
                                                return int.TryParse(id, out parsedId) ? (int?)parsedId : null;
                                            })
                                             .Where(id => id.HasValue)
                                             .Select(id => id!.Value)
                                             .ToList();

            if (!taskIdListParsed.Any())
            {
                TempData["DeleteError"] = "Selected ids to remove are invalid";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            try
            {
                bool successOperation = await _taskBulkOperations.RemoveMany(taskIdListParsed, authUser.Id!.Value);

                if (!successOperation)
                {
                    TempData["DeleteError"] = "Remove operation had an error. Make sure the Tasks are valid.";
                    return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
                }

                TempData["SuccessOperation"] = "Tasks Selected were successfully removed";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../TaskHome/RemoveMany");

                TempData["DeleteError"] = "Remove operation had a fatal error";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeManyStatus
            (TaskBulkViewModel viewModel)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
            {
                _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

                TempData["AuthError"] = "Unauthorized access";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(viewModel.IdList))
            {
                TempData["DeleteError"] = "Selected Tasks to update are invalid";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            var taskIdListParsed = viewModel.IdList.Split(',')
                                                .Select(id => {
                                                    int parsedId;
                                                    return int.TryParse(id, out parsedId) ? (int?)parsedId : null;
                                                })
                                                 .Where(id => id.HasValue)
                                                 .Select(id => id!.Value)
                                                 .ToList();

            if (!taskIdListParsed.Any())
            {
                TempData["DeleteError"] = "Selected ids to remove are invalid";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            try
            {
                bool successOperation = await _taskBulkOperations
                                                .UpdateManyStatus
                                                (taskIdListParsed, authUser.Id!.Value, viewModel.Status!);

                if (!successOperation)
                {
                    TempData["DeleteError"] = @"Status update operation had an error. 
                                                Make sure you are not changing it to the same status.";
                    return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
                }

                TempData["SuccessOperation"] = "Tasks Selected were successfully removed";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../TaskHome/RemoveMany");

                TempData["DeleteError"] = "Remove operation had a fatal error";
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }
        }
    }
}
