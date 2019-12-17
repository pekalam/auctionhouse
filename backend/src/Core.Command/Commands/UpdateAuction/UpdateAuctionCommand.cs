using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.Command.Exceptions;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Common;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Products;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.UpdateAuction
{
    [AuthorizationRequired]
    public class UpdateAuctionCommand : ICommand
    {
        public Guid AuctionId { get; }

        //can be null
        public BuyNowPrice BuyNowPrice { get; }
        public AuctionDate EndDate { get; }
        //

        [MinCount(3)]
        [MaxCount(3)]
        [Required]
        public List<string> Category { get; }

        [MinLength(Product.DESCRIPTION_MIN_LENGTH)]
        [Required]
        public string Description { get; }

        [MinCount(AuctionConstantsFactory.DEFAULT_MIN_TAGS)]
        [Required]
        public Tag[] Tags { get; }

        [Required]
        public AuctionName Name { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }


        public UpdateAuctionCommand(Guid auctionId, 
            BuyNowPrice buyNowPrice, AuctionDate endDate, 
            List<string> category, string description, 
            Tag[] tags, AuctionName name)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            BuyNowPrice = buyNowPrice;
            EndDate = endDate;
            Category = category;
            Description = description;
            Tags = tags;
            Name = name;
        }
    }
}