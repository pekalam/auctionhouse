using Core.Common;
using Core.Common.Attributes;
using Core.Common.Domain.Users;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.User.UserAuctions
{
    [AuthorizationRequired]
    public class UserAuctionsQuery : IQuery<UserAuctionsQueryResult>
    {
        public int Page { get; set; } = 0;

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }
    }
}
