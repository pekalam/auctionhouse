using System.ComponentModel.DataAnnotations;
using Auctions.Domain;
using Common.Application.Queries;

namespace ReadModel.Core.Queries.Auction.CommonTags
{
    public class CommonTagsQuery : IQuery<Views.CommonTags>
    {
        [Required]
        [MinLength(TagConstants.MIN_LENGTH)]
        [MaxLength(TagConstants.MAX_LENGTH)]
        public string Tag { get; set; }
    }
}
