using Microsoft.AspNetCore.Mvc;
using ReadModel.Contracts.Queries.User.UserAuctions;

namespace Auctionhouse.Query.Queries
{
    public class UserWonAuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;

        [FromQuery(Name = "sort")] public UserAuctionsSorting Sorting { get; set; }

        [FromQuery(Name = "dir")] public UserAuctionsSorting SortingDirection { get; set; }
    }
}