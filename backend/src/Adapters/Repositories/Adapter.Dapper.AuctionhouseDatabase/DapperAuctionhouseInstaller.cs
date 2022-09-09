using Adapter.Dapper.AuctionhouseDatabase.UserPayments_;
using AuctionBids.Domain;
using AuctionBids.Domain.Repositories;
using Auctions.Domain;
using Auctions.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using UserPayments.Domain;
using UserPayments.Domain.Repositories;
using Users.Domain;
using Users.Domain.Repositories;

namespace Adapter.Dapper.AuctionhouseDatabase
{
    public static class DapperAuctionhouseInstaller
    {
        public static void AddAuctionhouseDatabaseRepositories(this IServiceCollection services, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            services.AddAuctionBidsModuleRepositories(settings);
            services.AddUserPaymentsModuleRepositories(settings);
        }

        public static AuctionsDomainInstaller AddDapperAuctionRepositoryAdapter(this AuctionsDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<MsSqlAuctionRepository>();
            installer.AddAuctionRepository((prov) => prov.GetRequiredService<MsSqlAuctionRepository>());

            return installer;
        }

        public static AuctionBidsDomainInstaller AddDapperAuctionBidsRepositoryAdapter(this AuctionBidsDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<MsSqlAuctionBidsRepository>();
            installer.AddAuctionBidsRepository((prov) => prov.GetRequiredService<MsSqlAuctionBidsRepository>());

            return installer;
        }

        public static UserPaymentsDomainInstaller AddDapperUserPaymentsRepositoryAdapter(this UserPaymentsDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<MsSqlUserPaymentsRepository>();
            installer.AddUserPaymentsRepository((prov) => prov.GetRequiredService<MsSqlUserPaymentsRepository>());

            return installer;
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

        public static UsersDomainInstaller AddDapperUserRepositoryAdapter(this UsersDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<MsSqlUserRepository>();
            installer.AddUserRepository((prov) => prov.GetRequiredService<MsSqlUserRepository>());

            return installer;
        }

        public static UsersDomainInstaller AddDapperUserAuthenticationDataRepositoryAdapter(this UsersDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<UserAuthenticationDataRepository>();
            installer.AddUserAuthenticationDataRepository((prov) => prov.GetRequiredService<UserAuthenticationDataRepository>());

            return installer;
        }
        public static UsersDomainInstaller AddDapperResetPasswordCodeRepositoryAdapter(this UsersDomainInstaller installer, IConfiguration? configuration = null, AuctionhouseRepositorySettings? settings = null)
        {
            settings ??= configuration!.GetSection(nameof(AuctionhouseRepositorySettings)).Get<AuctionhouseRepositorySettings>();
            installer.Services.AddSingleton(settings);

            installer.Services.AddTransient<ResetPasswordCodeRepository>();
            installer.AddResetPasswordCodeRepository((prov) => prov.GetRequiredService<ResetPasswordCodeRepository>());

            return installer;
        }
    }
}
