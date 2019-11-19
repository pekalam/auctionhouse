using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Query;
using Core.Query.Views;

namespace Core.Query.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByTagQuery : IQuery<TopAuctionsInTag>
    {
        public const int MAX_PER_PAGE = 20;

        [Required]
        public string Tag { get; set; }
        public int Page { get; set; } = 0;
    }
}