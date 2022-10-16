using Auctions.Domain.Services;
using Hangfire.SqlServer;
using Hangfire;

namespace Adapter.Hangfire_.Auctionhouse
{
    using Auctions.Domain;
    using Auctions.Domain.Repositories;
    using Common.Application;
    using Common.Application.Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics;

    internal class SqlServerSettings
    {
        public string ConnectionString { get; set; } = null!;
    }

    public static class HangfireAdapterInstaller
    {
        public static AuctionsDomainInstaller AddHangfireAuctionUnlockSchedulerAdapter(this AuctionsDomainInstaller installer, IConfiguration? configuration = null, string? connectionString = null)
        {
            installer.Services.AddHangfireAuctionUnlockSchedulerAdapter(configuration, connectionString);

            installer.AddAuctionUnlockScheduler((prov) => prov.GetRequiredService<AuctionUnlockScheduler>());
            return installer;
        }

        public static void AddHangfireAuctionUnlockSchedulerAdapter(this IServiceCollection services,
            IConfiguration? configuration = null, string? connectionString = null,
            //Test dependencies
            Func<IServiceProvider, IAuctionRepository>? auctionRepositoryFactory = null,
            Func<IServiceProvider, IEventOutbox>? eventOutboxFactory = null,
            Func<IServiceProvider, IUnitOfWorkFactory>? unitOfWorkFactory = null,
            Func<IServiceProvider, AuctionUnlockService>? auctionUnlockServiceFactory = null)
        {
            if(configuration is null && connectionString is null)
            {
                throw new ArgumentException("Invalid configuration");
            }

            services.AddCoreServices(configuration, connectionString);

            if (auctionRepositoryFactory != null && eventOutboxFactory != null && unitOfWorkFactory != null) 
            {
                services.AddTransient((prov) => auctionRepositoryFactory(prov));
                services.AddTransient((prov) => eventOutboxFactory(prov));
                services.AddTransient((prov) => unitOfWorkFactory(prov));

                if(auctionUnlockServiceFactory != null)
                {
                    services.AddTransient((prov) => auctionUnlockServiceFactory(prov));
                }
            }

            services.AddTransient<IAuctionUnlockSchedulerJobIdFinder, AuctionUnlockSchedulerJobIdFinder>();
            services.AddTransient<IAuctionUnlockScheduler, AuctionUnlockScheduler>();
        }

        private static void AddCoreServices(this IServiceCollection services, IConfiguration? configuration = null, string? connectionString = null)
        {
            connectionString ??= configuration!.GetSection(nameof(Hangfire))["ConnectionString"];
            services.AddTransient<AutomaticRetryAttribute>(s => new AutomaticRetryAttribute());
            services.AddSingleton(new SqlServerSettings
            {
                ConnectionString = connectionString
            });
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