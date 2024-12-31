using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Use_Cases.Cache;

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

    public static void AddRedisCache(this IServiceCollection services,
                                    IConfiguration config)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = config.GetSection("CacheSettings")["ConnectionString"]!;
            return ConnectionMultiplexer.Connect(connectionString);
        });

        services.AddSingleton<ICacheHandler, CacheHandler>();
    }

    public static void MigrateDB(this IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<TaskContext>();

            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred applying the migrations: {ex.Message}");
            }
        }
    }
}
