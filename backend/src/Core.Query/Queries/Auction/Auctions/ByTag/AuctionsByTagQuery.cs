using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Query;

namespace Core.Query.Queries.Auction.Auctions.ByTag
{
    public class AuctionsByTagQuery : AuctionsQueryBase, IQuery<AuctionsQueryResult>
    {
        [Range(0, int.MaxValue)]
        public int Page { get; set; } = 0;
        [Required]
        [MinLength(Common.Domain.Auctions.Tag.MIN_LENGTH)]
        [MaxLength(Common.Domain.Auctions.Tag.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
