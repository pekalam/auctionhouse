using System.ComponentModel.DataAnnotations;
using Common.Application.Queries;
using ReadModel.Contracts.Views;

namespace ReadModel.Contracts.Queries.Auction.Auctions.TopAuctions
{
    public class TopAuctionsByProductNameQuery : IQuery<TopAuctionsByProductName[]>
    {
        public const int MAX_PER_PAGE = 20;

        [Required]
        public string ProductName { get; set; }
        public int Page { get; set; } = 0;
    }
}