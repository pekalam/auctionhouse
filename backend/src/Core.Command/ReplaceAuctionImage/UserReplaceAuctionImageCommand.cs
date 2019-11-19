using System;
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
        public AuctionImageRepresentation Img { get; }
        public int ImgNum { get; }
        public CorrelationId CorrelationId { get; }

        public UserReplaceAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, int imgNum,
            CorrelationId correlationId)
        {
            AuctionId = auctionId;
            Img = img;
            ImgNum = imgNum;
            CorrelationId = correlationId;
        }
    }
}