using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Common;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Command.CreateAuction
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
        public UserIdentity SignedInUser { get; set; }

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
