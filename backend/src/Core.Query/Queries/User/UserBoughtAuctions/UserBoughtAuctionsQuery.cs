using System;
using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Domain.Users;
using Core.Common.Query;

namespace Core.Query.Queries.User.UserAuctions
{
    [AuthorizationRequired]
    public class UserBoughtAuctionsQuery : IQuery<UserBoughtAuctionQueryResult>
    {
        [Range(0, Int32.MaxValue)]
        public int Page { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public UserBoughtAuctionsQuery(int page = 0)
        {
            Page = page;
        }
    }
}