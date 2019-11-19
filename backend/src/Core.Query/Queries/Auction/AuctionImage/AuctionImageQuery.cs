using System.ComponentModel.DataAnnotations;
using Core.Common.Query;

namespace Core.Query.Queries.Auction.AuctionImage
{
    public class AuctionImageQuery : IQuery<AuctionImageQueryResult>
    {
        [Required]
        public string ImageId { get; set; }
    }
}
