using System;

namespace Users.Domain.Auth
{
    public class UserAuthenticationData
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
