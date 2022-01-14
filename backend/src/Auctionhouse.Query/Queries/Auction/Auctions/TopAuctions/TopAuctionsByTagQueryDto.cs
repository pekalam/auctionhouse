using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class TopAuctionsByTagQueryDto
    {
        [FromQuery(Name = "tag")]
        public string Tag { get; set; }

        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;
    }
}