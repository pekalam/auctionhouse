using Core.DomainFramework;
using System;

namespace Core.Common.Domain.UserPayments
{
    public sealed class AuctionId : GuidId<AuctionId>
    {
        public AuctionId(Guid value) : base(value)
        {
        }
        public static AuctionId New() => new AuctionId(Guid.NewGuid());
    }
}
