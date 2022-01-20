using Auctions.Domain.Services;
using Hangfire.SqlServer;
using Hangfire;

namespace Adapter.Hangfire_.Auctionhouse
{
    using Microsoft.Extensions.DependencyInjection;

    internal class SqlServerSettings
    {
        public string ConnectionString { get; set; } = null!;
    }

    public static class HangfireAdapterInstaller
    {
        public static void AddHangfireServices(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<AutomaticRetryAttribute>(s => new AutomaticRetryAttribute());
            services.AddSingleton(new SqlServerSettings
            {
                ConnectionString = connectionString
            });
            services.AddTransient<IAuctionUnlockSchedulerJobIdFinder, AuctionUnlockSchedulerJobIdFinder>();
            services.AddTransient<IAuctionUnlockScheduler, AuctionUnlockScheduler>();
            services.AddHangfire((provider, cfg) =>
            {
                cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseFilter(provider.GetRequiredService<AutomaticRetryAttribute>())
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString,
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true
                });
            });
            services.AddHangfireServer();
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.UseActivator(new Hangfire.AspNetCore.AspNetCoreJobActivator(serviceProvider.GetService<IServiceScopeFactory>()));
        }
    }
}