using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Application;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.User.UserData
{
    public class UserDataQueryHandler : QueryHandlerBase<UserDataQuery, UserDataQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IUserIdentityService _userIdentityService;

        public UserDataQueryHandler(ReadModelDbContext dbContext, IUserIdentityService userIdentityService)
        {
            _dbContext = dbContext;
            _userIdentityService = userIdentityService;
        }

        private Guid GetSignedInUserIdentity()
        {
            var user = _userIdentityService.GetSignedInUserIdentity();
            if (user == null)
            {
                throw new Exception("Null user identity");
            }
            return user;
        }

        protected async override Task<UserDataQueryResult> HandleQuery(UserDataQuery request, CancellationToken cancellationToken)
        {
            var userId = GetSignedInUserIdentity();

            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId, userId.ToString());
            var result = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new UserDataQueryResult() { Address = model.Address, Username = model.UserIdentity.UserName, Credits = model.Credits })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
