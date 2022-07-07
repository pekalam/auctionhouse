using System.Threading.Tasks;

namespace Core.Common.Domain.Users
{
    public interface IUsernameProfanityCheck //TODO
    {
        Task<bool> CheckIsSatisfyingConditions(string username);
    }
}