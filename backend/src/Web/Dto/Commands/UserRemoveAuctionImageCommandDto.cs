using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class UserRemoveAuctionImageCommandDto
    {
        [FromForm(Name = "auction-id")]
        public string AuctionId { get; set; }
        [FromForm(Name = "img-num")]
        public int ImgNum { get; set; }
        [FromForm(Name = "correlation-id")]
        public string CorrelationId { get; set; }
    }
}