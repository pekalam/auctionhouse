using Common.Application.Queries;

namespace ReadModel.Core.Queries.User.UserData
{
    [AuthorizationRequired]
    public class UserDataQuery : IQuery<UserDataQueryResult>
    {
    }
}
