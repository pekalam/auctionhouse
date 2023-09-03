using System.ComponentModel.DataAnnotations;
using Common.Application.Queries;
using ReadModel.Contracts.Views;

namespace ReadModel.Contracts.Queries.Auction.Auctions.TopAuctions
{
    public class TopAuctionsInTagQuery : IQuery<TopAuctionsInTag>
    {
        public const int MAX_PER_PAGE = 20;

        [Required]
        public string Tag { get; set; }
        public int Page { get; set; } = 0;


    }
}