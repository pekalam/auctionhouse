using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.AuctionhouseDatabase
{
    using AuctionBids.Domain;

    internal class MsSqlAuctionBidsRepository : IAuctionBidsRepository
    {
        public void Add(AuctionBids auctionBids)
        {
            throw new NotImplementedException();
        }

        public AuctionBids WithAuctionId(AuctionId auctionId)
        {
            throw new NotImplementedException();
        }
    }
}
