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
        public BuyNowPrice BuyNowPrice { get; set; }
        [Required]
        public Product Product { get; }
        [Required]
        public AuctionDate StartDate { get; }
        [Required]
        public AuctionDate EndDate { get; }
        [Required]
        [MinCount(3)]
        [MaxCount(3)]
        public List<string> Category { get; }

        [Required]
        public bool? BuyNowOnly { get; }

        [Required]
        [MinLength(AuctionConstantsFactory.DEFAULT_MIN_TAGS)]
        public Tag[] Tags { get; }

        [Required]
        public AuctionName Name { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public AuctionCreateSession AuctionCreateSession { get; set; }

        public CreateAuctionCommand(BuyNowPrice buyNowPrice, Product product,
            AuctionDate startDate, AuctionDate endDate, List<string> category,
            Tag[] tags, AuctionName name, bool? buyNowOnly)
        {
            BuyNowPrice = buyNowPrice;
            Product = product;
            StartDate = startDate;
            EndDate = endDate;
            Category = category;
            Tags = tags;
            Name = name;
            BuyNowOnly = buyNowOnly;
        }
    }
}
