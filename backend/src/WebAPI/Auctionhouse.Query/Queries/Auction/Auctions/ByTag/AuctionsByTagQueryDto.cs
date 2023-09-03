using Microsoft.AspNetCore.Mvc;
using ReadModel.Contracts.Queries.Auction.Auctions;
using ReadModel.Core.Queries.Auction.Auctions;

namespace Auctionhouse.Query.Queries
{
    public class AuctionsByTagQueryDto
    {
        [FromQuery(Name = "tag")]

        public string Tag { get; set; }
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;

        [FromQuery(Name = "cond")]
        public ConditionQuery ConditionQuery { get; set; } = ConditionQuery.All;
        [FromQuery(Name = "minbpr")]
        public decimal MinBuyNowPrice { get; set; } = 0;
        [FromQuery(Name = "maxbpr")]
        public decimal MaxBuyNowPrice { get; set; } = 0;
        [FromQuery(Name = "minapr")]
        public decimal MinAuctionPrice { get; set; } = 0;
        [FromQuery(Name = "maxapr")]
        public decimal MaxAuctionPrice { get; set; } = 0;
        [FromQuery(Name = "type")]
        public AuctionTypeQuery AuctionTypeQuery { get; set; } = AuctionTypeQuery.All;
    }
}
