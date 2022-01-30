using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Core.Command.Bid
{
    [AuthorizationRequired]
    public class RaiseBidCommand : ICommand
    {
        public Guid AuctionId { get; set; }
        public decimal Price { get; set; } //TODO money value obj to prevent invalid decimal places count

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public RaiseBidCommand(Guid auctionId, decimal price)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Price = price;
        }
    }
}
