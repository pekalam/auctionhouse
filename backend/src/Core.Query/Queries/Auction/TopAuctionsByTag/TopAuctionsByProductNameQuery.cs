using System.ComponentModel.DataAnnotations;
using Core.Common.Query;
using Core.Query.Views;
using Core.Query.Views.TopAuctionsByProductName;

namespace Core.Query.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByProductNameQuery : IQuery<TopAuctionsByProductName>
    {
        public const int MAX_PER_PAGE = 20;

        [Required]
        public string ProductName { get; set; }
        public int Page { get; set; } = 0;
    }
}