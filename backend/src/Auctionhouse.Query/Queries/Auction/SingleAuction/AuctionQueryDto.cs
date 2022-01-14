using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class AuctionQueryDto
    {
        [FromQuery(Name = "auctionId")]

        public string AuctionId { get; set; }
    }
}
