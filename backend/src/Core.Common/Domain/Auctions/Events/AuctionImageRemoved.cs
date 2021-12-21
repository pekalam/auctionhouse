using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionImageRemoved : Event
    {
        public Guid AuctionId { get; }
        public int ImgNum { get; }
        public Guid AuctionOwner { get; }

        public AuctionImageRemoved(Guid auctionId, int imgNum, Guid auctionOwner) : base(EventNames.AuctionImageRemoved)
        {
            AuctionId = auctionId;
            ImgNum = imgNum;
            AuctionOwner = auctionOwner;
        }

    }
}
