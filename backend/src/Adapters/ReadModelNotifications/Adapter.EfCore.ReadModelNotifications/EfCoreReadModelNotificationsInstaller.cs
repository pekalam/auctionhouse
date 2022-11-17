using Microsoft.EntityFrameworkCore;
using ReadModelNotifications;

namespace Adapter.EfCore.ReadModelNotifications
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class EfCoreReadModelNotificationsInstaller
    {
        public static void AddCommandEfCoreReadModelNotifications(this IServiceCollection services, IConfiguration configuration,
            EfCoreReadModelNotificaitonsOptions? settings = null)
        {
            AddCoreServices(services, configuration, settings);
            services.AddCommandReadModelNotifications<EfCoreSagaNotifications, EfCoreSagaNotifications>(configuration);
        }

        public static void AddQueryEfCoreReadModelNotifications(this IServiceCollection services, IConfiguration configuration,
            EfCoreReadModelNotificaitonsOptions? settings = null)
        {
            AddCoreServices(services, configuration, settings);
            services.AddQueryReadModelNotificiations<EfCoreSagaNotifications, EfCoreSagaNotifications>(configuration);
        }


        private static void AddCoreServices(IServiceCollection services, IConfiguration? configuration, EfCoreReadModelNotificaitonsOptions? settings)
        {
            settings ??= configuration!.GetSection(nameof(EfCoreReadModelNotificaitonsOptions)).Get<EfCoreReadModelNotificaitonsOptions>();
            services.AddSingleton(settings);
            services.AddDbContext<SagaEventsConfirmationDbContext>(opt =>
                ConfigureDbContext(settings, opt));
            services.AddScoped<EfCoreSagaNotifications>();
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