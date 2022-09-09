using Microsoft.Extensions.DependencyInjection;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Domain
{
    public class UsersDomainInstaller
    {
        public UsersDomainInstaller(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public UsersDomainInstaller AddResetPasswordCodeRepository(Func<IServiceProvider, IResetPasswordCodeRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public UsersDomainInstaller AddUserRepository(Func<IServiceProvider, IUserRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public UsersDomainInstaller AddUserAuthenticationDataRepository(Func<IServiceProvider, IUserAuthenticationDataRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public UsersDomainInstaller AddResetLinkSenderService(Func<IServiceProvider, IResetLinkSenderService> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
