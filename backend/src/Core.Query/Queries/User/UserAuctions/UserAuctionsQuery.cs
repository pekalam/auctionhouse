using MediatR;

namespace Core.Query.Queries.User.UserAuctions
{
    public class UserAuctionsQuery : IRequest<UserAuctionsQueryResult>
    {
        public int Page { get; set; } = 0;
    }
}
