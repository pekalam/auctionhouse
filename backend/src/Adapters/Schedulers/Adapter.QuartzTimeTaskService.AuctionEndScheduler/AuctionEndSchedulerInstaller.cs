using Auctions.Domain;
using Auctions.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuartzTimeTaskService.AuctionEndScheduler;
using RestEase;

namespace Adapter.QuartzTimeTaskService.AuctionEndScheduler
{
    public static class AuctionEndSchedulerInstaller
    {
        public static AuctionsDomainInstaller AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(this AuctionsDomainInstaller installer,
            Func<IServiceProvider, ITimeTaskClient> timeTaskClientFactory = null,
            IConfiguration? configuration = null,
            TimeTaskServiceSettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(TimeTaskServiceSettings)).Get<TimeTaskServiceSettings>();
            installer.Services.AddTransient(timeTaskClientFactory);
            installer.Services.AddSingleton(settings);
            installer.Services.AddTransient<QuartzAuctionEndScheduler>();

            installer.AddAuctionEndScheduler((prov) => prov.GetRequiredService<QuartzAuctionEndScheduler>());
            return installer;
        }

        public static AuctionsDomainInstaller AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(this AuctionsDomainInstaller installer, 
            IConfiguration? configuration = null, 
            TimeTaskServiceSettings? settings = null)
        {
            installer.AddQuartzTimeTaskServiceAuctionEndSchedulerAdapter(
                provider =>
                {
                    var client = RestClient.For<ITimeTaskClient>(settings.ConnectionString);
                    client.ApiKey = settings.ApiKey;
                    return client;
                },
                configuration, settings
                );

            return installer;
        }

        public static void AddQuartzTimeTaskServiceAuctionEndSchedulerServices(this IMvcBuilder builder)
        {
            builder.AddApplicationPart(typeof(AuctionEndSchedulerInstaller).Assembly).AddControllersAsServices();
        }

        public static void AddQuartzTimeTaskServiceAuth(this AuthenticationBuilder builder)
        {
            builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.Scheme,
                        null);
        }
    }
}
