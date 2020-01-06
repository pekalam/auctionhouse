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
        [Range(0, Int32.MaxValue)] public int Page { get; }

        public UserAuctionsSorting Sorting { get; }

        public UserAuctionsSortDir SortingDirection { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public UserWonAuctionsQuery(int page, UserAuctionsSorting sorting, UserAuctionsSortDir sortingDirection)
        {
            Page = page;
            Sorting = sorting;
            SortingDirection = sortingDirection;
        }
    }
}