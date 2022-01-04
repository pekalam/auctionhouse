using ReadModel.Core.Queries.User.UserAuctions;
using System.ComponentModel.DataAnnotations;
using Auctions.Domain;
using Common.Application.Commands.Attributes;
using Common.Application.Queries;

namespace ReadModel.Core.Queries.User.UserBoughtAuctions
{
    [AuthorizationRequired]
    public class UserBoughtAuctionsQuery : IQuery<UserBoughtAuctionQueryResult>
    {
        [Range(0, int.MaxValue)] public int Page { get; }

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