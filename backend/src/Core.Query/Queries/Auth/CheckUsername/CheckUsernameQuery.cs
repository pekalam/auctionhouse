using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Query;

namespace Core.Query.Queries.Auth.CheckUsername
{
    public class CheckUsernameQuery : IQuery<CheckUsernameQueryResult>
    {
        [MinLength(Common.Domain.Users.User.MIN_USERNAME_LENGTH)]
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
