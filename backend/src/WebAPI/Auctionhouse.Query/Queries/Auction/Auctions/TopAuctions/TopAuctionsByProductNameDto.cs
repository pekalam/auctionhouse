using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class TopAuctionsByProductNameDto
    {
        [FromQuery(Name = "product-name")]
        public string ProductName { get; set; }
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;
    }
}
