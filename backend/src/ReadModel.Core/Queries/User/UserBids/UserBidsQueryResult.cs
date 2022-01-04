using System.Collections.Generic;
using MediatR;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserBids
{
    public class UserBidsQueryResult : IRequest<Unit>
    {
        private static readonly UserBid[] _bids = new UserBid[0];

        public IEnumerable<UserBid> UserBids { get; set; } = _bids;
        public long Total { get; set; } = 0;
    }
}
