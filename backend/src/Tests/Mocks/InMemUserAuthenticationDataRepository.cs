using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Auth;
using Users.Domain.Repositories;

namespace FunctionalTests.Mocks
{
    public class InMemUserAuthenticationDataRepository : IUserAuthenticationDataRepository
    {
        private readonly Dictionary<Guid, UserAuthenticationData> _users = new();

        public UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData)
        {
            _users[userAuthenticationData.UserId] = userAuthenticationData;
            return userAuthenticationData;
        }

        public UserAuthenticationData FindUserAuth(string userName)
        {
            return _users.Values.FirstOrDefault(x => x.UserName == userName);
        }

        public UserAuthenticationData FindUserAuthByEmail(string email)
        {
            return _users.Values.FirstOrDefault(x => x.Email == email);
        }

        public UserAuthenticationData FindUserAuthById(Guid id)
        {
            return _users.Values.FirstOrDefault(x => x.UserId == id);
        }

        public void UpdateUserAuth(UserAuthenticationData userAuthenticationData)
        {
            _users[userAuthenticationData.UserId] = userAuthenticationData;
        }
    }
}
