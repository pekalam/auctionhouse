using Common.Application.Commands;

namespace Auctions.Application.Commands.AssignAuctionBids
{
    public class AssignAuctionBidsCommand : ICommand
    {
        public Guid AuctionBidsId { get; set; }
        public Guid AuctionId { get; set; }
    }
}
