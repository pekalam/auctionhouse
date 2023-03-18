using Microsoft.Extensions.DependencyInjection;
using Users.Application;
using Users.Domain;

namespace Users.DI
{
    public class UsersModuleInstaller
    {
        public UsersModuleInstaller(IServiceCollection services)
        {
            Domain = new UsersDomainInstaller(services);
            Application = new UserApplicationInstaller(services);
        }

        public UserApplicationInstaller Application { get; }

        public UsersDomainInstaller Domain { get; }
    }
}