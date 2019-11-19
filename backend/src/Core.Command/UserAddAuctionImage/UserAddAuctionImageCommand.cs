using System;
using System.Collections.Generic;
using System.Text;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.AddOrReplaceAuctionImage
{
    [AuthorizationRequired]
    public class UserAddAuctionImageCommand : ICommand
    {
        public Guid AuctionId { get; }
        public AuctionImageRepresentation Img { get; }
        public CorrelationId CorrelationId { get; }

        public UserAddAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, CorrelationId correlationId)
        {
            AuctionId = auctionId;
            Img = img;
            CorrelationId = correlationId;
        }
    }
}
