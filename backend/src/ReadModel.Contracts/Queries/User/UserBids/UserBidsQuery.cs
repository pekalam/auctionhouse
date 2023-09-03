using Auctions.Domain;
using Common.Application.Commands.Attributes;
using Common.Application.Queries;
using System;

namespace ReadModel.Contracts.Queries.User.UserBids
{
    [AuthorizationRequired]
    public class UserBidsQuery : IQuery<UserBidsQueryResult>
    {
        public int Page { get; set; } = 0;

        [SignedInUser]
        public Guid SignedInUser { get; set; }
    }
}
