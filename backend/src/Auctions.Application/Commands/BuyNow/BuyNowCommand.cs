using Common.Application.Commands;
using Common.Application.Commands.Attributes;

namespace Auctions.Application.Commands.BuyNow
{
    [AuthorizationRequired]
    public class BuyNowCommand : ICommand
    {
        public Guid AuctionId { get; set; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }
    }
}
