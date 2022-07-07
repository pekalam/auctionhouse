using Core.DomainFramework;

namespace Auctions.Domain
{
    public class AuctionBidsId : GuidId<AuctionBidsId>
    {
        public AuctionBidsId(Guid value) : base(value)
        {
        }
    }
}