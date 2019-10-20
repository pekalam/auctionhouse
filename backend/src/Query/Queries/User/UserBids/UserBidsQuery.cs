using MediatR;

namespace Core.Query.Queries.User.UserBids
{
    public class UserBidsQuery : IRequest<UserBidsQueryResult>
    {
        public int Page { get; set; } = 0;
    }
}
