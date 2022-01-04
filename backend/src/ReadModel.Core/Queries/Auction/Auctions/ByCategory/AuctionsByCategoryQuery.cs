using System.Collections.Generic;
using Auctions.Application.CommandAttributes;
using Common.Application.Queries;
using System.ComponentModel.DataAnnotations;

namespace ReadModel.Core.Queries.Auction.Auctions.ByCategory
{
    public class AuctionsByCategoryQuery : AuctionsQueryBase, IQuery<AuctionsQueryResult>
    {
        [Range(0, int.MaxValue)]
        public int Page { get; set; } = 0;
        [Required]
        [MinCount(1)]
        public List<string> CategoryNames { get; set; }
    }
}
