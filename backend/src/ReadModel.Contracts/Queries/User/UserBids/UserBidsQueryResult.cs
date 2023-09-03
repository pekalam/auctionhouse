using System.Collections.Generic;
using MediatR;
using ReadModel.Contracts.Model;

namespace ReadModel.Contracts.Queries.User.UserBids
{
    public class UserBidsQueryResult : IRequest<Unit>
    {
        private static readonly UserBid[] _bids = new UserBid[0];

        public IEnumerable<UserBid> UserBids { get; set; } = _bids;
        public long Total { get; set; } = 0;
    }
}
