using Auctions.Application.CommandAttributes;
using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.UpdateAuction
{
    [AuthorizationRequired]
    public class UpdateAuctionCommand : ICommand
    {
        [Required]
        public Guid AuctionId { get; set; }

        public BuyNowPrice? BuyNowPrice { get; set; }
        public AuctionDate? EndDate { get; set; }

        [MinCount(3)]
        [MaxCount(3)]
        public List<string>? Category { get; set; }

        [MinLength(Product.DESCRIPTION_MIN_LENGTH)]
        public string? Description { get; set; }

        [MinCount(AuctionConstantsFactory.DEFAULT_MIN_TAGS)]
        public Tag[]? Tags { get; set; }

        public AuctionName? Name { get; set; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }
    }
}