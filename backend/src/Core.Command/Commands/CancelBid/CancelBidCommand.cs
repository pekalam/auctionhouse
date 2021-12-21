using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.Commands.CancelBid
{
    [AuthorizationRequired]
    public class CancelBidCommand : CommandBase
    {
        public Guid BidId { get; }
        public Guid AuctionId { get; }

        [SignedInUser]
        public UserId SignedInUser { get; set; }

        public CancelBidCommand(Guid bidId, Guid auctionId)
        {
            BidId = bidId;
            AuctionId = auctionId;
        }
    }
}
