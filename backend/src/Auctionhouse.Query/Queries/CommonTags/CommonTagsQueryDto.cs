using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Query.Queries
{
    public class CommonTagsQueryDto
    {
        [FromQuery(Name = "tag")]
        public string Tag { get; set; }
    }
}
