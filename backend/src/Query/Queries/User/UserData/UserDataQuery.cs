using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace Core.Query.Queries.User.UserData
{
    public class UserDataQuery : IRequest<UserDataQueryResult>
    {
    }
}
