using Core.Common.Domain.Auctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class UserReplaceAuctionImageCommandDto
    {
        [FromForm(Name = "auction-id")]
        public string AuctionId { get; set; }
        [FromForm(Name = "img")]
        public IFormFile Img { get; set; }
        [FromForm(Name = "img-num")]
        public int ImgNum { get; set; }
        [FromForm(Name = "correlation-id")]
        public string CorrelationId { get; set; }
    }
}
