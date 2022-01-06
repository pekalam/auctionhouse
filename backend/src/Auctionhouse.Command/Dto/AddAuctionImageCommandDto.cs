using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Command.Dto
{
    public class AddAuctionImageCommandDto
    {
        [FromForm(Name = "img")]
        public IFormFile Img { get; set; }
        [FromForm(Name = "img-num")]
        public int ImgNum { get; set; }
    }
}
