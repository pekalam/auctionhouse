using System.ComponentModel.DataAnnotations;

namespace Auctionhouse.Command.Dto
{
    public class BuyNowCommandDto
    {
        [Required]
        public string AuctionId { get; set; }
    }
}
