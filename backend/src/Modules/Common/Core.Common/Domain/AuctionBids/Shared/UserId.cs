using Core.DomainFramework;
using System;

namespace Core.Common.Domain.AuctionBids
{
    public sealed class UserId : GuidId<UserId>
    {
        public UserId(Guid value) : base(value) { }
        public static UserId New() => new UserId(Guid.NewGuid());
    }
}
