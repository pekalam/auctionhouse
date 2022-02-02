using Adapter.Dapper.AuctionhouseDatabase.UserPayments_;
using AuctionBids.Domain.Repositories;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserPayments.Domain.Repositories;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    public static class DapperAuctionhouseInstaller
    {
        public static void AddAuctionhouseDatabaseRepositories(this IServiceCollection services, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            services.AddUserModuleRepositories(settings);
            services.AddAuctionBidsModuleRepositories(settings);
            services.AddAuctionModuleRepositories(settings);
            services.AddUserPaymentsModuleRepositories(settings);
        }

        public static void AddUserPaymentsModuleRepositories(this IServiceCollection services, AuctionhouseRepositorySettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<IUserPaymentsRepository, MsSqlUserPaymentsRepository>();
        }

        public static void AddAuctionBidsModuleRepositories(this IServiceCollection services, AuctionhouseRepositorySettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<IAuctionBidsRepository, MsSqlAuctionBidsRepository>();
        }

        public static void AddAuctionModuleRepositories(this IServiceCollection services, AuctionhouseRepositorySettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<IAuctionRepository, MsSqlAuctionRepository>();
        }

        public static void AddUserModuleRepositories(this IServiceCollection services, AuctionhouseRepositorySettings settings)
        {
            services.AddSingleton(settings);
            services.AddTransient<IUserRepository, MsSqlUserRepository>();
            services.AddTransient<IUserAuthenticationDataRepository, UserAuthenticationDataRepository>();
            services.AddTransient<IResetPasswordCodeRepository, ResetPasswordCodeRepository>();
        }
    }
}
