using Auctions.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using QuartzTimeTaskService.AuctionEndScheduler;
using RestEase;

namespace Adapter.QuartzTimeTaskService.AuctionEndScheduler
{
    public static class AuctionEndSchedulerInstaller
    {
        public static void AddQuartzTimeTaskServiceAuctionEndScheduler(this IServiceCollection services, TimeTaskServiceSettings settings)
        {
            services.AddSingleton(provider =>
            {
                var client = RestClient.For<ITimeTaskClient>(settings.ConnectionString);
                client.ApiKey = settings.ApiKey;
                return client;
            });
            services.AddSingleton(settings);
            services.AddTransient<IAuctionEndScheduler, QuartzAuctionEndScheduler>();
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
