using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionImageRemoved : Event
    {
        public int ImgNum { get; }

        public AuctionImageRemoved(int imgNum) : base(EventNames.AuctionImageRemoved)
        {
            ImgNum = imgNum;
        }
    }
}
