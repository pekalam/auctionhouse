using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Adapter.Dapper.AuctionhouseDatabase;

namespace Users.DI
{
    public static class InstallationExtensions
    {
        public static UsersModuleInstaller AddUsersModule(this IServiceCollection services, IConfiguration configuration)
        {
            var installer = new UsersModuleInstaller(services);
            installer
                .Domain
                    .AddDapperResetPasswordCodeRepositoryAdapter(configuration)
                    .AddDapperUserRepositoryAdapter(configuration)
                    .AddDapperUserAuthenticationDataRepositoryAdapter(configuration);

            return installer;
        }
    }
}