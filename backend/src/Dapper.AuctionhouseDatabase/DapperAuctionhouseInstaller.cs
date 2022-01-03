using Auctions.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Users.Domain.Repositories;

namespace Dapper.AuctionhouseDatabase
{
    public static class DapperAuctionhouseInstaller
    {
        public static void AddDapperAuctionhouse(this IServiceCollection services)
        {
            services.AddTransient<IAuctionRepository, MsSqlAuctionRepository>();
            services.AddTransient<IUserRepository, MsSqlUserRepository>();
        }
    }
}
