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
        [Range(0, Int32.MaxValue)] public int Page { get; }

        public UserAuctionsSorting Sorting { get; }

        public UserAuctionsSortDir SortingDirection { get; }

        [SignedInUser]
        public UserId SignedInUser { get; set; }

        public UserBoughtAuctionsQuery(int page, UserAuctionsSorting sorting, UserAuctionsSortDir sortingDirection)
        {
            Page = page;
            Sorting = sorting;
            SortingDirection = sortingDirection;
        }
    }
}