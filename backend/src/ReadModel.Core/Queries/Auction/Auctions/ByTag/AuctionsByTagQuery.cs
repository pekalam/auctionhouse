using Common.Application.Queries;
using System.ComponentModel.DataAnnotations;


namespace ReadModel.Core.Queries.Auction.Auctions.ByTag
{

    public class AuctionsByTagQuery : AuctionsQueryBase, IQuery<AuctionsQueryResult>
    {
        [Range(0, int.MaxValue)]
        public int Page { get; set; } = 0;
        [Required]
        [MinLength(global::Auctions.Domain.Tag.MIN_LENGTH)]
        [MaxLength(global::Auctions.Domain.Tag.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
