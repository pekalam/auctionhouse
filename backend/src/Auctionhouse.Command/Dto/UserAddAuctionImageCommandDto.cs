using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class UserAddAuctionImageCommandDto
    {
        [FromForm(Name = "auction-id")]
        public string AuctionId { get; set; }
        [FromForm(Name = "img")]
        public IFormFile Img { get; set; }
    }
}
