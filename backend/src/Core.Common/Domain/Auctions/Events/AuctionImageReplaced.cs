using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Users;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionImageReplaced : Event
    {
        public Guid AuctionId { get; }
        public int ImgNum { get; }
        public AuctionImage NewImage { get; }
        public UserIdentity AuctionOwner { get; }

        public AuctionImageReplaced(Guid auctionId, int imgNum, AuctionImage newImage, UserIdentity auctionOwner) : base(EventNames.AuctionImageReplaced)
        {
            AuctionId = auctionId;
            ImgNum = imgNum;
            NewImage = newImage;
            AuctionOwner = auctionOwner;
        }
    }
}
