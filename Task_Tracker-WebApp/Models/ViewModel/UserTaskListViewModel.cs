namespace Task_Tracker_WebApp.Models.ViewModel
{
    public class UserTaskListViewModel
    {
        public IEnumerable<UserTaskModel> Tasks { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalTasks { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public UserTaskListViewModel
            (IEnumerable<UserTaskModel> tasks,
            int pageNumber,
            int pageSize,
            int totalPages,
            int totalTasks)
        {
            Tasks = tasks;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = totalPages;
            TotalTasks = totalTasks;
        }
    }
}
