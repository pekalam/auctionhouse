using Microsoft.AspNetCore.Mvc;
using ReadModel.Core.Queries.User.UserAuctions;

namespace Auctionhouse.Query.Queries
{
    public class UserAuctionsQueryDto
    {
        [FromQuery(Name = "page")] public int Page { get; set; } = 0;

        [FromQuery(Name = "show-archived")] public bool ShowArchived { get; set; }

        [FromQuery(Name = "sort")] public UserAuctionsSorting Sorting { get; set; }

        [FromQuery(Name = "dir")] public UserAuctionsSortDir SortingDirection { get; set; }
    }
}