using Common.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using UserPayments.Domain.Repositories;
using UserPayments.Domain.Services;

namespace UserPayments.Application
{
    public class UserPaymentsApplicationInstaller
    {
        public UserPaymentsApplicationInstaller(IServiceCollection services)
        {
            Services = services;
            services.AddEventSubscribers(typeof(UserPaymentsApplicationInstaller));
            services.AddTransient<PaymentMethodVerificationService>();
        }

        public IServiceCollection Services { get; }
    }
}
