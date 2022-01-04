using Auctions.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using QuartzTimeTaskService.AuctionEndScheduler;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            services.AddTransient<IAuctionEndScheduler, AuctionSchedulerService>();
        }
    }
}
