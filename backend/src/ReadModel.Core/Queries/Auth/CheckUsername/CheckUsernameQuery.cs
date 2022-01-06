using System.ComponentModel.DataAnnotations;
using Common.Application.Queries;
using Core.Common.Domain.Users;

namespace ReadModel.Core.Queries.Auth.CheckUsername
{
    public class CheckUsernameQuery : IQuery<CheckUsernameQueryResult>
    {
        [MinLength(UserConstants.MIN_USERNAME_LENGTH)]
        public string Username { get; }

        public CheckUsernameQuery(string username)
        {
            Username = username;
        }
    }

    public class CheckUsernameQueryResult
    {
        public string Username { get; set; }
        public bool Exist { get; set; }
    }
}
