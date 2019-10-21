using System;

namespace Core.Common.Domain.Users
{
    public class UserIdentity
    {
        public static readonly UserIdentity Empty = new UserIdentity();

        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public AuctionCreateSession.AuctionCreateSession GetAuctionCreateSession()
        {
            return new AuctionCreateSession.AuctionCreateSession(this);
        }
    }
}