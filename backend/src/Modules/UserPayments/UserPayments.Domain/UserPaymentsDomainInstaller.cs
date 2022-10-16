using Microsoft.Extensions.DependencyInjection;
using UserPayments.Application;
using UserPayments.Domain.Repositories;

namespace UserPayments.Domain
{
    public class UserPaymentsDomainInstaller
    {
        public UserPaymentsDomainInstaller(IServiceCollection services)
        {
            Services = services;
            services.AddTransient<IPaymentMethodRepository, InMemoryPaymentMethodRepository>();
        }

        public IServiceCollection Services { get; }

        public UserPaymentsDomainInstaller AddPaymentMethodRepository(Func<IServiceProvider, IPaymentMethodRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }

        public UserPaymentsDomainInstaller AddUserPaymentsRepository(Func<IServiceProvider, IUserPaymentsRepository> factory)
        {
            Services.AddTransient(factory);
            return this;
        }
    }
}
