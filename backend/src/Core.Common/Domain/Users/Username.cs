using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Common.Domain.Users.UsernameProfanity;

namespace Core.Common.Domain.Users
{
    public class Username : ValueObject
    {
        public string Value { get; private set; }

        internal Username(string value)
        {
            Value = value;
        }

        public static async Task<Username> Create(string username, IUsernameProfanityCheck profanityCheck = null) //TODO
        {
            if (profanityCheck == null) profanityCheck = new NullUsernameProfanityCheck();


            if (username.Length < User.MIN_USERNAME_LENGTH)
            {
                throw new InvalidUsernameException("Too short username");
            }
            if (await profanityCheck.CheckIsSatisfyingConditions(username))
            {
                return new Username(username);
            }

            throw new UsernameProfanityFoundException(username, $"Found username profanity: {username}");
        }

        public override string ToString() => Value;
        public static implicit operator string(Username username) => username.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}