namespace Task_Tracker_WebApp.Use_Cases.Auth.Models
{
    public class AuthorizedUser
    {
        public int? Id { get; set; }
        public string? UserName { get; set; }
        public bool Valid { get; }

        public AuthorizedUser(string? id, string? username)
        {
            if (int.TryParse(id, out int idNumber))
                Id = idNumber;
            else
                Id = null;

            UserName = username;
            Valid = id is not null && username is not null;
        }
    }
}
