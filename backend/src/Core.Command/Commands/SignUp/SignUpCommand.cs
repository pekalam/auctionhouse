using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Core.Common.Command;
using Core.Common.Domain.Users;

namespace Core.Command.SignUp
{
    public class RegexAttribute : ValidationAttribute
    {
        private Regex _regex;

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

    public class SignUpCommand : ICommand    {
        [Required]
        [MinLength(User.MIN_USERNAME_LENGTH)]
        public string Username { get; }
        [Required]
        [Regex(@"(?=.*[a-z])(?!.*[\!\@\#\$\%\^\&\*\+\=\)\(_\`\~\'\""\,\.\|])(?=.{4,})")]
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
