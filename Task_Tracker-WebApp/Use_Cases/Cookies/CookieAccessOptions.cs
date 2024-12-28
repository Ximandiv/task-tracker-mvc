namespace Task_Tracker_WebApp.Use_Cases.Auth
{
    public class CookieAccessOptions(IConfiguration config)
    {
        public CookieOptions GetAccessOptions()
        {
            int accessExpMins;
            if (!int.TryParse(config.GetSection("JwtSettings")["ExpirationInMinutes"], out accessExpMins))
                accessExpMins = 1;

            return new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(accessExpMins)
            };
        }

        public CookieOptions GetRememberOptions()
        {
            int rememberExpMonths;
            if (!int.TryParse(config.GetSection("JwtSettings")["RememberExpInMonths"], out rememberExpMonths))
                rememberExpMonths = 2;

            return new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMonths(rememberExpMonths)
            };
        }
    }
}
