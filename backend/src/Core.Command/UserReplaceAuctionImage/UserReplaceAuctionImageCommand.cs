using System;
using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.ReplaceAuctionImage
{
    [AuthorizationRequired]
    public class UserReplaceAuctionImageCommand : ICommand
    {
        public Guid AuctionId { get; }
        [Required]
        public AuctionImageRepresentation Img { get; }
        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }
        [Required]
        public CorrelationId CorrelationId { get; }

        public UserReplaceAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, int imgNum,
            CorrelationId correlationId)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Img = img;
            ImgNum = imgNum;
            CorrelationId = correlationId;
        }
    }
}