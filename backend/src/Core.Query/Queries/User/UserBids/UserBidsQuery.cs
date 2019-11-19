using Core.Common;
using Core.Common.Attributes;
using Core.Common.Domain.Users;
using Core.Common.Query;

namespace Core.Query.Queries.User.UserBids
{
    [AuthorizationRequired]
    public class UserBidsQuery : IQuery<UserBidsQueryResult>
    {
        public int Page { get; set; } = 0;

        [SignedInUser]
        public UserIdentity SignedInUser { get; set; }
    }
}
