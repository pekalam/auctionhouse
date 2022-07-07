using Core.DomainFramework;
using System;

namespace Core.Common.Domain.AuctionBids
{
    public sealed class AuctionId : GuidId<AuctionId>
    {
        public AuctionId(Guid value) : base(value) { }
        public static AuctionId New() => new AuctionId(Guid.NewGuid());
    }
}
