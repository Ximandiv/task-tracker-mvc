using Task_Tracker_WebApp.Extension_Methods;
using Task_Tracker_WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

string dbConnectionString = builder.GetDBConnectionString();

builder.ConfigureJWT();

builder.Services.AddMySqlContext(dbConnectionString);

builder.Services.AddRepositories();
builder.Services.AddUseCases();

builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();
builder.Services.AddCacheHandler();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.Services.MigrateDB();

app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<JWTSessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
