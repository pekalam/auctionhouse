using System.Collections.Generic;
using Core.Query.ReadModel;
using MediatR;

namespace Core.Query.Queries.User.UserBids
{
    public class UserBidsQueryResult : IRequest<Unit>
    {
        private static UserBid[] _bids = new UserBid[0];

        public IEnumerable<UserBid> UserBids { get; set; } = _bids;
        public long Total { get; set; } = 0;
    }
}
