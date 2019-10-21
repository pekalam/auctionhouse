using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common.Common;
using MediatR;

namespace Core.Query.Queries.Auction.Auctions
{
    public enum Condition
    {
        Used, New, All
    }

    public enum AuctionType
    {
        BuyNow, Auction, All
    }

    public class AuctionsQuery : IRequest<IEnumerable<AuctionsQueryResult>>
    {
        public int Page { get; set; } = 0;
        [Required]
        [MinCount(1)]
        public List<string> CategoryNames { get; set; }

        public Condition Condition { get; set; } = Condition.All;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
        public AuctionType AuctionType { get; set; } = AuctionType.All;
    }
}
