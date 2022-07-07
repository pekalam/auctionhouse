using UserPayments.Domain.Repositories;
using UserPayments.Domain.Shared;

namespace UserPayments.Domain.Services
{
    public class PaymentMethodVerificationService
    {
        private readonly IPaymentMethodRepository _paymentMethods;

        public PaymentMethodVerificationService(IPaymentMethodRepository paymentMethods)
        {
            _paymentMethods = paymentMethods;
        }

        public async Task<bool> Verify(PaymentMethod paymentMethod, UserId userId)
        {
            var existingPaymentMethod = await _paymentMethods.WithName(paymentMethod.Name);
            return existingPaymentMethod != null;
        }
    }
}
