using Microsoft.Extensions.DependencyInjection;
using UserPayments.Application;
using UserPayments.Domain;

namespace UserPayments.DI
{
    public class UserPaymentsInstaller
    {
        public UserPaymentsInstaller(IServiceCollection services)
        {
            Domain = new UserPaymentsDomainInstaller(services);
            Application = new UserPaymentsApplicationInstaller(services);
        }

        public UserPaymentsDomainInstaller Domain { get; }
        public UserPaymentsApplicationInstaller Application { get; }
    }
}