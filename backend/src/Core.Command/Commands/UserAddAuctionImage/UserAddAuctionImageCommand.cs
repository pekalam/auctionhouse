using System;
using System.ComponentModel.DataAnnotations;
using Core.Command.Exceptions;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.UserAddAuctionImage
{
    [AuthorizationRequired]
    public class UserAddAuctionImageCommand : ICommand
    {
        public Guid AuctionId { get; }
        [Required]
        public AuctionImageRepresentation Img { get; }

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }

        public UserAddAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Img = img;
        }
    }
}
