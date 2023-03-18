using Adapter.Dapper.AuctionhouseDatabase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserPayments.DI;

public static class InstallationExtensions
{
    public static void AddUserPaymentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        new UserPaymentsModuleInstaller(services)
            .Domain
                .AddDapperUserPaymentsRepositoryAdapter(configuration);
    }
}