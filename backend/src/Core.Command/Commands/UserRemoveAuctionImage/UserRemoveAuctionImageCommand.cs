using System;
using System.ComponentModel.DataAnnotations;
using Core.Command.Exceptions;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;

namespace Core.Command.Commands.UserRemoveAuctionImage
{
    [AuthorizationRequired]
    public class UserRemoveAuctionImageCommand : ICommand    {
        public Guid AuctionId { get; }
        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        public UserRemoveAuctionImageCommand(Guid auctionId, int imgNum)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            ImgNum = imgNum;
        }
    }
}