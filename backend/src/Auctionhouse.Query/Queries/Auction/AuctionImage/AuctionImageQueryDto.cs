using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class AuctionImageQueryDto
    {
        [FromQuery(Name = "img")]
        public string ImageId { get; set; }
    }
}
