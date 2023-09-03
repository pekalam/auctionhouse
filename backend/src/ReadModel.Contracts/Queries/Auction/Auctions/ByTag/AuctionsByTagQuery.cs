using Auctions.Domain;
using Common.Application.Queries;
using ReadModel.Contracts.Queries.Auction.Auctions;
using System.ComponentModel.DataAnnotations;


namespace ReadModel.Contracts.Queries.Auction.Auctions.ByTag
{

    public class AuctionsByTagQuery : AuctionsQueryBase, IQuery<AuctionsQueryResult>
    {
        [Range(0, int.MaxValue)]
        public int Page { get; set; } = 0;
        [Required]
        [MinLength(TagConstants.MIN_LENGTH)]
        [MaxLength(TagConstants.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
