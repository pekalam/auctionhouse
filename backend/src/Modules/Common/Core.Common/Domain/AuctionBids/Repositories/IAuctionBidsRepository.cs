using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.AuctionBids.Repositories
{
    public interface IAuctionBidsRepository
    {
        AuctionBids WithAuctionId(AuctionId auctionId);
        void Add(AuctionBids auctionBids);
    }
}
