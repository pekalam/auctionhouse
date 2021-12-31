using Core.DomainFramework;

namespace AuctionBids.Domain.Shared
{
    public sealed class AuctionId : GuidId<AuctionId>
    {
        public AuctionId(Guid value) : base(value) { }
        public static AuctionId New() => new AuctionId(Guid.NewGuid());
    }
}
