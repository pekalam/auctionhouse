using Core.Common.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain;
using Users.Domain.Repositories;

namespace Test.Users.Base.Mocks
{
    public class InMemoryUserRepository : IUserRepository
    {
        private Dictionary<UserId, User> _users = new Dictionary<UserId,User>();

        public User AddUser(User user)
        {
            _users[user.AggregateId] = user;
            return user;
        }

        public User? FindUser(UserId userId)
        {
            return _users.ContainsKey(userId.Value) ? _users[userId] : null;
        }

        public void UpdateUser(User user)
        {
            _users[user.AggregateId] = user;
        }
    }
}
