using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Use_Cases.Auth;
using Task_Tracker_WebApp.Use_Cases.Tasks;
using Task_Tracker_WebApp.Use_Cases.Tasks.Enum;

namespace Task_Tracker_WebApp.Controllers
{
    [Authorize]
    public class TaskHomeController
        (CredentialsHandler authService,
        TaskOperations taskOps,
        TaskRetrieval taskRetrieval,
        ILogger<TaskHomeController> logger) : Controller
    {
        private readonly ILogger<TaskHomeController> _logger = logger;
        private readonly CredentialsHandler _authService = authService;
        private readonly TaskOperations _taskOperations = taskOps;
        private readonly TaskRetrieval _taskRetrieval = taskRetrieval;

        [HttpGet]
        public async Task<IActionResult> Dashboard
            (int pageNumber = 1,
            int filterNumber = (int)UserTaskStatus.Unknown)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
                return kickUnauthorized();

            UserTaskListViewModel? viewModelResult = null;
            try
            {
                viewModelResult = await _taskRetrieval.GetUserTasksByFilter(authUser, pageNumber, filterNumber);
            }
            catch(Exception ex)
            {
                return redirectError<string>(ex, 
                                $"GET /TaskHome/Dashboard?pageNumber={pageNumber}&filterNumber={filterNumber}",
                                "Error during Dashboard Task List retrieval");
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
                return kickUnauthorized();

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
                return kickUnauthorized();

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
                return redirectError(ex,
                            "POST /TaskHome/Create",
                            "An error occurred during Task Creation",
                            nameof(Create),
                            "GeneralError",
                            taskViewModel);
            }

            TempData["SuccessOperation"] = "Task was successfully created";
            return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int taskId)
        {
            var authUser = _authService.GetAuthUser(User);

            if (!authUser.Valid)
                return kickUnauthorized();

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
                return kickUnauthorized();

            string error = string.Empty;
            try
            {
                error = await _taskOperations.Update(authUser, model.UserTask!);
            }
            catch(Exception ex)
            {
                return redirectError(ex,
                    $"POST /TaskHome/Edit?taskId={model.UserTask!.Id}",
                    "An error occurred during task edition",
                    nameof(Edit),
                    "GeneralError",
                    model);
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
                return kickUnauthorized();

            string error = string.Empty;
            try
            {
                error = await _taskOperations.Remove(authUser, taskId);
            }
            catch (Exception ex)
            {
                return redirectError(ex,
                    $"POST /TaskHome/Remove?taskId={taskId}",
                    "An error occurred during task deletion",
                    "TaskHome",
                    "GeneralError",
                    new { pageNumber = TempData["ActualPage"] },
                    "Dashboard"
                    );
            }

            if(!string.IsNullOrEmpty(error))
            {
                TempData["DeleteError"] = error;
                return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
            }

            TempData["SuccessOperation"] = "Task was successfully removed";
            return RedirectToAction("Dashboard", new { pageNumber = TempData["ActualPage"] });
        }
        
        private IActionResult kickUnauthorized()
        {
            _logger.LogWarning(
                    @$"User tried to get in dashboard with invalid auth values");

            TempData["AuthError"] = "Unauthorized access";
            return RedirectToAction("Index", "Home");
        }

        private IActionResult redirectError<T>
            (Exception ex,
            string endpoint,
            string viewMsg,
            string? viewName = null,
            string? dataKey = null,
            T? viewObject = null,
            string? actionName = null) where T : class
        {
            _logger.LogError(ex, $"An error occurred in {endpoint}");

            if (viewName is not null
                && actionName is not null)
            {
                TempData[dataKey!] = viewMsg;
                return RedirectToAction(actionName, viewName, viewObject);
            }
            else if(viewName is not null
                && actionName is null)
            {
                ViewData[dataKey!] = viewMsg;
                return View(viewName, viewObject);
            }

            ViewData["AuthError"] = viewMsg;
            return RedirectToAction("Index", "Home");
        }
    }
}
