using MongoDB.Driver;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModel.Core.Queries.Auction
{
    internal static class NotLockedFilterExtension
    {
        public static FilterDefinition<AuctionRead> AuctionIsNotLocked(this FilterDefinitionBuilder<AuctionRead> filterDefinition)
        {
            return filterDefinition.Eq(a => a.Locked, false);
        }
    }
}
