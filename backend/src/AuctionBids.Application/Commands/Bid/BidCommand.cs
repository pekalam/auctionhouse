using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Core.Command.Bid
{
    [AuthorizationRequired]
    public class BidCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public BidCommand(Guid auctionId, decimal price)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Price = price;
        }
    }
}
