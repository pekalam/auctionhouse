using Common.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Users.Application
{
    public class UserApplicationInstaller
    {
        public UserApplicationInstaller(IServiceCollection services)
        {
            Services = services;
            services.AddEventSubscribers(typeof(UserApplicationInstaller));
        }
        public IServiceCollection Services { get; }
    }
}
