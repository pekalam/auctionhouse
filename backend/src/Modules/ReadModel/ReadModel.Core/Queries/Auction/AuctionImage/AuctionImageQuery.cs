using Common.Application.Queries;
using System.ComponentModel.DataAnnotations;

namespace ReadModel.Core.Queries.Auction.AuctionImage
{
    public class AuctionImageQuery : IQuery<AuctionImageQueryResult>
    {
        [Required]
        public string ImageId { get; set; }
    }
}
