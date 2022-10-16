using Microsoft.Extensions.DependencyInjection;
using Users.Application;
using Users.Domain;

namespace Users.DI
{
    public class UsersInstaller
    {
        public UsersInstaller(IServiceCollection services)
        {
            Domain = new UsersDomainInstaller(services);
            Application = new UserApplicationInstaller(services);
        }

        public UserApplicationInstaller Application { get; }

        public UsersDomainInstaller Domain { get; }
    }
}