using Task_Tracker_WebApp.Auth;
using Task_Tracker_WebApp.Cache;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

string dbConnectionString = builder.GetDBConnectionString();

builder.ConfigureJWT();

builder.Services.AddMySqlContext(dbConnectionString);

builder.Services.AddSingleton<JWTGenerator>();
builder.Services.AddSingleton<MemoryCacheHandler>();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<JWTSessionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
