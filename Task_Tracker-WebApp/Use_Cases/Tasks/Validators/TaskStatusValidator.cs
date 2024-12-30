using Task_Tracker_WebApp.Use_Cases.Tasks.Enum;

namespace Task_Tracker_WebApp.Use_Cases.Tasks.Validators
{
    public static class TaskStatusValidator
    {
        public static bool ValidStatus(int statusNumber, out string status)
        {
            switch (statusNumber)
            {
                case (int)UserTaskStatus.ToDo :
                    status = "To-Do";
                    return true;
                case (int)UserTaskStatus.InProgress :
                    status = "In-Progress";
                    return true;
                case (int)UserTaskStatus.Complete :
                    status = "Completed";
                    return true;
                default:
                    status = "N/A";
                    return false;
            }
        }
    }
}
