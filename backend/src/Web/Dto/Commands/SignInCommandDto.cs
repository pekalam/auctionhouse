using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Commands
{
    public class SignInCommandDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
