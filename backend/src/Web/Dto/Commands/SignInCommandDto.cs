using System.ComponentModel.DataAnnotations;

namespace Web.Dto.Commands
{
    public class SignInCommandDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
