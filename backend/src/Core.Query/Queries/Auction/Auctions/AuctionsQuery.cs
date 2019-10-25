using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common.Common;
using MediatR;

namespace Core.Query.Queries.Auction.Auctions
{
    public enum ConditionQuery
    {
        Used, New, All
    }

    public enum AuctionTypeQuery
    {
        BuyNow, Auction, All
    }

    public class AuctionsQuery : IRequest<IEnumerable<AuctionsQueryResult>>
    {
        public int Page { get; set; } = 0;
        [Required]
        [MinCount(1)]
        public List<string> CategoryNames { get; set; }

        public ConditionQuery ConditionQuery { get; set; } = ConditionQuery.All;
        public decimal MinBuyNowPrice { get; set; } = 0;
        public decimal MaxBuyNowPrice { get; set; } = 0;
        public AuctionTypeQuery AuctionTypeQuery { get; set; } = AuctionTypeQuery.All;
    }
}
