using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Auctions.Application.Commands.BuyNow
{
    [AuthorizationRequired]
    public class BuyNowCommand : ICommand
    {
        public Guid AuctionId { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public BuyNowCommand(Guid auctionId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
        }
    }
}
