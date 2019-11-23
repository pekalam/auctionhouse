using System;
using System.Linq;
using Core.Common.Auth;
using MongoDB.Driver;

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
            var found = _dbContext.UserAuth.Find(u => u.UserId == id).FirstOrDefault();
            return UserAuthenticationDataAssembler.From(found);
        }

        public virtual UserAuthenticationData FindUserAuth(string userName)
        {
            var found = _dbContext.UserAuth.Find(u => u.UserName == userName).FirstOrDefault();
            return UserAuthenticationDataAssembler.From(found);
        }

        public virtual UserAuthenticationData AddUserAuth(UserAuthenticationData userAuthenticationData)
        {
            _dbContext.UserAuth.InsertOne(UserAuthenticationDataAssembler.From(userAuthenticationData));
            return userAuthenticationData;
        }

        public void SaveUserAuth(UserAuthenticationData userAuthenticationData)
        {
            var filter = Builders<UserAuthenticationDataMongo>.Filter.Eq(auth => auth.UserId, userAuthenticationData.UserId);
            var update =
                Builders<UserAuthenticationDataMongo>.Update.Set(auth => auth.Password,
                    userAuthenticationData.Password);


            _dbContext.UserAuth.UpdateOne(filter, update);
        }
    }
}