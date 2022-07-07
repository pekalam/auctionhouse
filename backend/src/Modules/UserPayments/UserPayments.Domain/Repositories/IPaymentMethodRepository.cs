namespace UserPayments.Domain.Repositories
{
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod?> WithName(string name);
    }
}
