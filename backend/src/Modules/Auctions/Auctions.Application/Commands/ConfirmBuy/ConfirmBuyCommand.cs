using Common.Application.Commands;

namespace Auctions.Application.Commands.ConfirmBuy
{
    public class ConfirmBuyCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
