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
    using System.Data.Common;
    using System.Data.SqlClient;

    internal class SqlServerSettings
    {
        public string ConnectionString { get; set; } = null!;
    }

    public static class HangfireAuctionBuyCancellationSchedulerInstaller
    {
        public static AuctionsDomainInstaller AddHangfireAuctionBuyCancellationSchedulerAdapter(this AuctionsDomainInstaller installer, IConfiguration configuration)
        {
            return installer.AddHangfireAuctionBuyCancellationSchedulerAdapter(configuration, null);
        }


        public static AuctionsDomainInstaller AddHangfireAuctionBuyCancellationSchedulerAdapter(this AuctionsDomainInstaller installer, string connectionString)
        {
            return installer.AddHangfireAuctionBuyCancellationSchedulerAdapter(null, connectionString);
        }

        public static AuctionsDomainInstaller AddHangfireAuctionBuyCancellationSchedulerAdapter(this AuctionsDomainInstaller installer,
            IConfiguration? configuration, string? connectionString,
            //Test dependencies
            Func<IServiceProvider, IAuctionRepository>? auctionRepositoryFactory = null,
            Func<IServiceProvider, IEventOutbox>? eventOutboxFactory = null,
            Func<IServiceProvider, IUnitOfWorkFactory>? unitOfWorkFactory = null,
            Func<IServiceProvider, AuctionBuyCancellationService>? auctionBuyCancellationServiceFactory = null)
        {
            if(configuration is null && connectionString is null)
            {
                throw new ArgumentException("Invalid configuration");
            }

            installer.Services.AddCoreServices(configuration, connectionString);

            if (auctionRepositoryFactory != null && eventOutboxFactory != null && unitOfWorkFactory != null) 
            {
                installer.Services.AddTransient((prov) => auctionRepositoryFactory(prov));
                installer.Services.AddTransient((prov) => eventOutboxFactory(prov));
                installer.Services.AddTransient((prov) => unitOfWorkFactory(prov));

                if(auctionBuyCancellationServiceFactory != null)
                {
                    installer.Services.AddTransient((prov) => auctionBuyCancellationServiceFactory(prov));
                }
            }

            installer.Services.AddTransient<IAuctionCancelSchedulerJobIdFinder, AuctionCancelSchedulerJobIdFinder>();
            installer.Services.AddTransient<AuctionBuyCancellationScheduler>();

            installer.AddAuctionCancelScheduler((prov) => prov.GetRequiredService<AuctionBuyCancellationScheduler>());
            return installer;
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
                .UseSqlServerStorage(() => new Microsoft.Data.SqlClient.SqlConnection(connectionString), new SqlServerStorageOptions
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