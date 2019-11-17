using System;

namespace Core.Common.Auth
{
    public class UserAuthenticationData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
