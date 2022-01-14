using Microsoft.AspNetCore.Mvc;
using ReadModel.Core.Queries.User.UserAuctions;

namespace Auctionhouse.Query.Queries
{
    public class UserBoughtAuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;

        [FromQuery(Name = "sort")] public UserAuctionsSorting Sorting { get; set; }

        [FromQuery(Name = "dir")] public UserAuctionsSorting SortingDirection { get; set; }
    }
}
