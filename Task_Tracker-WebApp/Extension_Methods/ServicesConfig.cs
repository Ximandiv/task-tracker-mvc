using Task_Tracker_WebApp.Repositories.Instances;
using Task_Tracker_WebApp.Repositories.Interfaces;
using Task_Tracker_WebApp.Repositories;
using Task_Tracker_WebApp.Auth;
using Task_Tracker_WebApp.Use_Cases.Auth;
using Task_Tracker_WebApp.Use_Cases.Tasks;
using Task_Tracker_WebApp.Use_Cases.Cache;

namespace Task_Tracker_WebApp.Extension_Methods
{
    public static class ServicesConfig
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void AddUseCases(this IServiceCollection services)
        {
            services.AddSingleton<CookieAccessOptions>();
            services.AddSingleton<TokenGenerator>();

            services.AddScoped<TokenHandler>();
            services.AddScoped<CredentialsHandler>();
            services.AddScoped<TaskRetrieval>();
            services.AddScoped<TaskOperations>();
        }

        public static void AddCacheHandler(this IServiceCollection services)
            => services.AddSingleton<IMemoryCacheHandler, MemoryCacheHandler>();
    }
}
