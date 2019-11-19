using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Query;

namespace Core.Query.Queries.Auction.Auctions.ByTag
{
    public class AuctionsByTagQuery : AuctionsQueryBase, IQuery<IEnumerable<AuctionsQueryResult>>
    {
        public int Page { get; set; } = 0;
        [Required]
        public string Tag { get; set; }
    }
}
