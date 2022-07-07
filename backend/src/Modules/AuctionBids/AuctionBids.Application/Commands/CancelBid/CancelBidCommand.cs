using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Core.Command.Commands.CancelBid
{
    [AuthorizationRequired]
    public class CancelBidCommand : ICommand
    {
        public Guid BidId { get; }
        public Guid AuctionId { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public CancelBidCommand(Guid bidId, Guid auctionId)
        {
            BidId = bidId;
            AuctionId = auctionId;
        }
    }
}
