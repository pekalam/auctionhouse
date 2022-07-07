using System.Threading.Tasks;

namespace Core.Common.Domain.Users.UsernameProfanity
{
    class NullUsernameProfanityCheck : IUsernameProfanityCheck
    {
        public Task<bool> CheckIsSatisfyingConditions(string username)
        {
            return Task.FromResult(true);
        }
    }
}