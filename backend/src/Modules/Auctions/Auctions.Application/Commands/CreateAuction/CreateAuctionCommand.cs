using Auctions.Application.CommandAttributes;
using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.CreateAuction
{
    [AuthorizationRequired]
    [InAuctionCreateSessionRemove]
    public class CreateAuctionCommand : ICommand
    {
        public BuyNowPrice? BuyNowPrice { get; set; }

        [Required]
        public Product Product { get; set; } = null!;
        [Required]
        public AuctionDate StartDate { get; set; } = null!;
        [Required]
        public AuctionDate EndDate { get; set; } = null!;
        [Required]
        [MinCount(3)]
        [MaxCount(3)]
        public List<string> Category { get; set; } = null!;

        [Required]
        public bool BuyNowOnly { get; set; }

        [Required]
        [MinLength(AuctionConstantsFactory.DEFAULT_MIN_TAGS)]
        public Tag[] Tags { get; set; } = null!;

        [Required]
        public AuctionName Name { get; set; } = null!;

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public AuctionCreateSession AuctionCreateSession { get; set; } = null!;
    }
}
