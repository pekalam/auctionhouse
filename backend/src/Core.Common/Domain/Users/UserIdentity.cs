using System;
using System.Collections.Generic;

namespace Core.Common.Domain.Users
{
    public class UserIdentity : ValueObject
    {
        public static readonly UserIdentity Empty = new UserIdentity();

        public UserIdentity(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

        public UserIdentity()
        {
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public AuctionCreateSession.AuctionCreateSession GetAuctionCreateSession()
        {
            return new AuctionCreateSession.AuctionCreateSession(this);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return UserId;
            yield return UserName;
        }

        public override string ToString() => $"Username: {UserName}, UserId: {UserId}";
    }
}