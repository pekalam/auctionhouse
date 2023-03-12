using Common.Application.Commands;

namespace Auctions.Application.Commands.CancelBuy
{
    public class CancelBuyCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
