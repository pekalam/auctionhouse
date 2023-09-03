using MongoDB.Driver;
using ReadModel.Contracts.Model;

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
