using System;

namespace ReadModel.Contracts.Model
{
    public class UserIdentityRead
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public UserIdentityRead()
        {

        }

        public UserIdentityRead(Guid userId, string username)
        {
            UserId = userId.ToString();
            UserName = username;
        }
    }
}