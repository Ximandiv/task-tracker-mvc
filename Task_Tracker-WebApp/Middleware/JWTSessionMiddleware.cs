namespace Task_Tracker_WebApp.Middleware
{
    public class JWTSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public JWTSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Cookies["JWToken"];

            if(!string.IsNullOrEmpty(token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }
}
