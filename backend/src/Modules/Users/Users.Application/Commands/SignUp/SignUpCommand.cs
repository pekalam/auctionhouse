using Common.Application.Commands;
using Core.Common.Domain.Users;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Users.Application.Commands.SignUp
{
    using static UserConstants;

    public class RegexAttribute : ValidationAttribute
    {
        private readonly Regex _regex;

        public RegexAttribute(string pattern)
        {
            _regex = new Regex(pattern);
        }

        public override bool IsValid(object value)
        {
            var str = Convert.ToString(value, CultureInfo.CurrentCulture);
            var valid = _regex.IsMatch(str);
            return valid;
        }
    }

    public class SignUpCommand : ICommand
    {
        [Required]
        [MinLength(MIN_USERNAME_LENGTH)]
        public string Username { get; }
        [Required]
        [Regex(@"(?=.*[a-z])(?=.{4,})")]
        public string Password { get; }
        [Required]
        [EmailAddress]
        public string Email { get; }

        public SignUpCommand(string username, string password, string email)
        {
            Username = username;
            Password = password;
            Email = email;
        }
    }
}
