using System.Collections.Generic;
using Core.Query.ReadModel;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionsQueryResult
    {
        private static AuctionReadModel[] _auctions = new AuctionReadModel[0];

        public IEnumerable<AuctionReadModel> Auctions { get; set; } = _auctions;
    }
}
