using Auctions.Domain.Services;
using Common.Application.Commands;

namespace Auctions.Application.Commands.CreateAuction
{
    public class EndCreateAuctionCommand : ICommand
    {
        public Guid AuctionBidsId { get; set; }

        public CreateAuctionServiceData CreateAuctionServiceData { get; set; }
    }
}
