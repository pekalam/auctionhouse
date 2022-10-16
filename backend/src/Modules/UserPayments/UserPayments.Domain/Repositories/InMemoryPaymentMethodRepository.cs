using UserPayments.Domain;
using UserPayments.Domain.Repositories;

namespace UserPayments.Application
{
    internal class InMemoryPaymentMethodRepository : IPaymentMethodRepository
    {
        private static readonly List<PaymentMethod> _paymentMethods = new List<PaymentMethod>() {
            new("test")
        };

        public Task<PaymentMethod?> WithName(string name)
        {
            return Task.FromResult(_paymentMethods.FirstOrDefault(p => p.Name == name));
        }
    }
}
