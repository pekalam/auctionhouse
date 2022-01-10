using Adapter.Dapper.AuctionhouseDatabase.UserPayments_;
using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using UserPayments.Domain.Repositories;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    public static class DapperAuctionhouseInstaller
    {
        public static void AddDapperAuctionhouse(this IServiceCollection services, MsSqlConnectionSettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<IAuctionRepository, MsSqlAuctionRepository>();
            services.AddTransient<IUserRepository, MsSqlUserRepository>();
            services.AddTransient<IAuctionBidsRepository, MsSqlAuctionBidsRepository>();
            services.AddTransient<IUserAuthenticationDataRepository, UserAuthenticationDataRepository>();
            services.AddTransient<IResetPasswordCodeRepository, ResetPasswordCodeRepository>();
            services.AddTransient<IUserPaymentsRepository, MsSqlUserPaymentsRepository>();
        }
    }
}
