using System;
using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Domain.Users;
using Core.Common.Query;
using Core.Query.Queries.User.UserAuctions;

namespace Core.Query.Queries.User.UserWonAuctions
{
    [AuthorizationRequired]
    public class UserWonAuctionsQuery : IQuery<UserWonAuctionQueryResult>
    {
        [Range(0, Int32.MaxValue)]
        public int Page { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public UserWonAuctionsQuery(int page = 0)
        {
            Page = page;
        }
    }
}