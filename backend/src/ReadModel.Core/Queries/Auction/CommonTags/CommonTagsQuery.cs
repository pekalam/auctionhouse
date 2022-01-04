using System.ComponentModel.DataAnnotations;
using Common.Application.Queries;

namespace ReadModel.Core.Queries.Auction.CommonTags
{
    public class CommonTagsQuery : IQuery<Views.CommonTags>
    {
        [Required]
        [MinLength(Common.Domain.Auctions.Tag.MIN_LENGTH)]
        [MaxLength(Common.Domain.Auctions.Tag.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
