namespace ReadModel.Core.Queries.Auction.Auctions
{
    public class AuctionsQueryBase
    {
        public ConditionQuery ConditionQuery { get; set; } = ConditionQuery.All;
        public decimal MinBuyNowPrice { get; set; } = 0;
        public decimal MaxBuyNowPrice { get; set; } = 0;
        public decimal MinAuctionPrice { get; set; } = 0;
        public decimal MaxAuctionPrice { get; set; } = 0;
        public AuctionTypeQuery AuctionTypeQuery { get; set; } = AuctionTypeQuery.All;
    }
}