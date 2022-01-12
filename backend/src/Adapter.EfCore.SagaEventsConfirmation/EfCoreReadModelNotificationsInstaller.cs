using Microsoft.EntityFrameworkCore;

namespace Adapter.EfCore.ReadModelNotifications
{
    using Common.Application.SagaNotifications;
    using Microsoft.Extensions.DependencyInjection;

    public static class EfCoreReadModelNotificationsInstaller
    {
        public static void AddEfCoreReadModelNotifications(this IServiceCollection services, EfCoreReadModelNotificaitonsOptions settings)
        {
            services.AddDbContext<SagaEventsConfirmationDbContext>(opt =>
                ConfigureDbContext(settings, opt));
            services.AddTransient<ISagaNotifications, EfCoreSagaNotifications>();
            services.AddTransient<IImmediateNotifications, EfCoreSagaNotifications>();
        }

        private static DbContextOptionsBuilder ConfigureDbContext(EfCoreReadModelNotificaitonsOptions settings, DbContextOptionsBuilder opt)
        {
            opt.EnableSensitiveDataLogging(true);
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