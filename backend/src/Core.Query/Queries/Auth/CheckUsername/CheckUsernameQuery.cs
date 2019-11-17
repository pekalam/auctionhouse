using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.Queries.Auth.CheckUsername
{
    public class CheckUsernameQuery : IRequest<CheckUsernameQueryResult>
    {
        public string Username { get; }

        public CheckUsernameQuery(string username)
        {
            Username = username;
        }
    }

    public class CheckUsernameQueryResult
    {
        public string Username { get; set; }
        public bool Exist { get; set; }
    }

    public class CheckUsernameQueryHandler : IRequestHandler<CheckUsernameQuery, CheckUsernameQueryResult>
    {
        private ReadModelDbContext _dbContext;
        private ILogger<CheckUsernameQueryHandler> _logger;

        public CheckUsernameQueryHandler(ReadModelDbContext dbContext, ILogger<CheckUsernameQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CheckUsernameQueryResult> Handle(CheckUsernameQuery request,
            CancellationToken cancellationToken)
        {
            var filter = Builders<UserRead>.Filter.Eq(read => read.UserIdentity.UserName, request.Username);

            var count = await _dbContext.UsersReadModel.CountDocumentsAsync(filter);

            if (count > 0)
            {
                if (count > 1)
                {
                    _logger.LogCritical($"Duplicate users {request.Username}");
                }

                return new CheckUsernameQueryResult() {Exist = true, Username = request.Username};
            }
            else
            {
                return new CheckUsernameQueryResult() { Exist = false, Username = request.Username };
            }
        }
    }
}
