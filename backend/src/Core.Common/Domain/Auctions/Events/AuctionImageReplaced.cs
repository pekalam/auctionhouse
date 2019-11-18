using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Auctions.Events
{
    public class AuctionImageReplaced : Event
    {
        public int ImgNum { get; }
        public AuctionImage NewImage { get; }

        public AuctionImageReplaced(int imgNum, AuctionImage newImage) : base(EventNames.AuctionImageReplaced)
        {
            ImgNum = imgNum;
            NewImage = newImage;
        }
    }
}
