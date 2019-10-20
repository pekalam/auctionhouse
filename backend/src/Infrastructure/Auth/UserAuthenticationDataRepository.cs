using System;
using System.Linq;
using Core.Common.Auth;

namespace Infrastructure.Auth
{
    public class UserAuthenticationDataRepository : IUserAuthenticationDataRepository
    {
        private readonly UsertAuthDbContext _dbContext;

        public UserAuthenticationDataRepository(UsertAuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual UserAuthenticationData FindUserAuthById(Guid id)
        {
            var found = _dbContext.UserAuth.FirstOrDefault(u => u.UserId == id);
            return found;
        }

        public virtual UserAuthenticationData FindUserAuth(string userName)
        {
            var found = _dbContext.UserAuth.FirstOrDefault(u => u.UserName == userName);
            return found;
        }

        public virtual UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var added = _dbContext.UserAuth.Add(userAuthenticationData).Entity;
            _dbContext.SaveChanges();
            return added;
        }
    }
}