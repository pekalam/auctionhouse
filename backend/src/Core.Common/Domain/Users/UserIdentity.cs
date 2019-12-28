using System;

namespace Core.Common.Domain.Users
{
    public class UserIdentity
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

        public override bool Equals(object obj) => obj is UserIdentity && ((UserIdentity) obj).UserId.Equals(UserId);
        public override int GetHashCode() => UserId.GetHashCode();
        public override string ToString() => $"Username: {UserName}, UserId: {UserId}";
    }
}