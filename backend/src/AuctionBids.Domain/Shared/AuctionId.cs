using Core.DomainFramework;

namespace AuctionBids.Domain.Shared
{
    public sealed class AuctionId : GuidId<AuctionId>
    {
        public AuctionId(Guid value) : base(value) { }
    }
}
