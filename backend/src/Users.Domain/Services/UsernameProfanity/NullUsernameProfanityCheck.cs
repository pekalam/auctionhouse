namespace Users.Domain.Services.UsernameProfanity
{
    class NullUsernameProfanityCheck : IUsernameProfanityCheck
    {
        public Task<bool> CheckIsSatisfyingConditions(string username)
        {
            return Task.FromResult(true);
        }
    }
}