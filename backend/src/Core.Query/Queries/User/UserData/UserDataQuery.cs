using System;
using System.Collections.Generic;
using System.Text;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.User.UserData
{
    [AuthorizationRequired]
    public class UserDataQuery : IQuery<UserDataQueryResult>
    {
    }
}
