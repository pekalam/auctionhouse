using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auth.CheckUsername
{
    public class CheckUsernameQueryHandler : QueryHandlerBase<CheckUsernameQuery, CheckUsernameQueryResult>
    {
        private ReadModelDbContext _dbContext;
        private ILogger<CheckUsernameQueryHandler> _logger;

        public CheckUsernameQueryHandler(ReadModelDbContext dbContext, ILogger<CheckUsernameQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected async override Task<CheckUsernameQueryResult> HandleQuery(CheckUsernameQuery request, CancellationToken cancellationToken)
        {
            var filter = Builders<UserRead>.Filter.Eq(read => read.UserIdentity.UserName, request.Username);

            var count = await _dbContext.UsersReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                if (count > 1)
                {
                    _logger.LogCritical($"Duplicate users {request.Username}");
                }

                return new CheckUsernameQueryResult() { Exist = true, Username = request.Username };
            }
            else
            {
                return new CheckUsernameQueryResult() { Exist = false, Username = request.Username };
            }
        }
    }
}