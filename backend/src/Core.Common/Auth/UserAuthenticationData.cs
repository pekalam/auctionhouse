using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Common.Auth
{
    public class UserAuthenticationData
    {
        [Key]
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
