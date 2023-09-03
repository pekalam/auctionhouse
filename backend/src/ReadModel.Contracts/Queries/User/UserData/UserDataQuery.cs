using Common.Application.Commands.Attributes;
using Common.Application.Queries;

namespace ReadModel.Contracts.Queries.User.UserData
{
    [AuthorizationRequired]
    public class UserDataQuery : IQuery<UserDataQueryResult>
    {
    }
}
