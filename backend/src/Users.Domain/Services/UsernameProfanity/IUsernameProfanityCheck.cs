namespace Users.Domain.Services.UsernameProfanity
{
    public interface IUsernameProfanityCheck //TODO
    {
        Task<bool> CheckIsSatisfyingConditions(string username);
    }
}