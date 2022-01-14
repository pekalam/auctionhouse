using System.ComponentModel.DataAnnotations;

namespace Auctionhouse.Command.Dto
{
    public class UpdateAuctionCommandDto
    {
        [Required]
        public string AuctionId { get; set; }

        public decimal? BuyNowPrice { get; set; }
        public DateTime? EndDate { get; set; }

        public List<string>? Category { get; set; }
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public string? Name { get; set; }
    }

}
