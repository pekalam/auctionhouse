using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.UserRemoveAuctionImage
{
    [AuthorizationRequired]
    public class UserRemoveAuctionImageCommand : ICommand
    {
        public Guid AuctionId { get; }
        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public UserRemoveAuctionImageCommand(Guid auctionId, int imgNum)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandDataException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            ImgNum = imgNum;
        }
    }
}