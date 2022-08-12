using Microsoft.EntityFrameworkCore;
using ReadModelNotifications;

namespace Adapter.EfCore.ReadModelNotifications
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class EfCoreReadModelNotificationsInstaller
    {
        public static void AddEfCoreReadModelNotifications(this IServiceCollection services, IConfiguration? configuration = null, 
            EfCoreReadModelNotificaitonsOptions? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(EfCoreReadModelNotificaitonsOptions)).Get<EfCoreReadModelNotificaitonsOptions>();
            services.AddSingleton(settings);
            services.AddDbContext<SagaEventsConfirmationDbContext>(opt =>
                ConfigureDbContext(settings, opt));
            services.AddScoped<EfCoreSagaNotifications>();
            services.AddReadModelNotifications<EfCoreSagaNotifications, EfCoreSagaNotifications>();
        }

        private static DbContextOptionsBuilder ConfigureDbContext(EfCoreReadModelNotificaitonsOptions settings, DbContextOptionsBuilder opt)
        {
            //opt.EnableSensitiveDataLogging(true);
            if (settings.Provider.ToLower() == "sqlite")
            {
                return opt.UseSqlite(settings.ConnectionString);
            }
            else if (settings.Provider.ToLower() == "sqlserver")
            {
                return opt.UseSqlServer(settings.ConnectionString);
            }
            else throw new NotImplementedException();
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<SagaEventsConfirmationDbContext>().Database.EnsureCreated();
        }
    }
}