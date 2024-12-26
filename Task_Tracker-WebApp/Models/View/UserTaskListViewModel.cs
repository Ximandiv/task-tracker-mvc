using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Task_Tracker_WebApp.Models.View
{
    public class UserTaskListViewModel
    {
        public required IEnumerable<UserTaskViewModel> Tasks { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalTasks { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
