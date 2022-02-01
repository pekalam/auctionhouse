using Common.Application.Commands;

namespace UserPayments.Application.Commands.CreateBuyNowPayment
{
    public class CreateBuyNowPaymentCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public Guid BuyerId { get; set; }
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodName { get; set; }
    }
}
