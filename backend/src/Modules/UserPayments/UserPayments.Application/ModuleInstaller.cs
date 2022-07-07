using Common.Application;
using Microsoft.Extensions.DependencyInjection;
using UserPayments.Domain.Repositories;
using UserPayments.Domain.Services;

namespace UserPayments.Application
{
    public static class ModuleInstaller
    {
        public static void AddUserPaymentsModule(this IServiceCollection services)
        {
            services.AddEventSubscribers(typeof(ModuleInstaller));
            services.AddTransient<IPaymentMethodRepository, InMemoryPaymentMethodRepository>();
            services.AddTransient<PaymentMethodVerificationService>();
        }
    }
}
