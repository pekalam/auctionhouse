using Microsoft.AspNetCore.Mvc;

namespace Auctionhouse.Command.Dto
{
    public class RemoveImageCommandDto
    {
        [FromQuery(Name = "num")]
        public int ImgNum { get; set; }
    }
}
