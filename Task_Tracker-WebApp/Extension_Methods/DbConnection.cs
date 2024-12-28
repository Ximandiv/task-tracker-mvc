using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Database;

namespace Task_Tracker_WebApp.Extension_Methods;

public static class DbConnection
{
    public static string GetDBConnectionString(this WebApplicationBuilder builder)
    {
        string dbKeyEnvironment = "Production";
        if (builder.Environment.IsDevelopment())
            dbKeyEnvironment = "Test";

        string connectionString = builder.Configuration
                                    .GetConnectionString(dbKeyEnvironment)!;

        return connectionString;
    }
    public static void AddMySqlContext(this IServiceCollection services,
                                        string connectionString)
    {
        services.AddDbContext<TaskContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }
}
