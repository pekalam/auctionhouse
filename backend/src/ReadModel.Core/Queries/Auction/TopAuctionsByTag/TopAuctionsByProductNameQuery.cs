using System.ComponentModel.DataAnnotations;
using Common.Application.Queries;
using ReadModel.Core.Views;

namespace ReadModel.Core.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByProductNameQuery : IQuery<TopAuctionsByProductName>
    {
        public const int MAX_PER_PAGE = 20;

        [Required]
        public string ProductName { get; set; }
        public int Page { get; set; } = 0;
    }
}