using Core.DomainFramework;
using System;

namespace UserPayments.Domain.Shared
{
    public sealed class UserId : GuidId<UserId>
    {
        public UserId(Guid value) : base(value)
        {
        }
        public static UserId New() => new UserId(Guid.NewGuid());
    }
}
