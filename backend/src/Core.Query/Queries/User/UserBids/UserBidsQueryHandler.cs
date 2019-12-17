using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Domain.Users;
using Core.Common.Query;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.User.UserBids
{
    public class UserBidsQueryHandler : QueryHandlerBase<UserBidsQuery, UserBidsQueryResult>
    {
        private readonly ReadModelDbContext _dbContext;
        public const int PageSize = 10;

        public UserBidsQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<UserBidsQueryResult> HandleQuery(UserBidsQuery request, CancellationToken cancellationToken)
        {
            var userReadModelFilter = Builders<UserRead>.Filter.Eq(field => field.UserIdentity.UserId, request.SignedInUser.UserId.ToString());
            var result = await _dbContext.UsersReadModel
                .Find(userReadModelFilter)
                .Project(model => new UserBidsQueryResult() { UserBids = model.UserBids })
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
