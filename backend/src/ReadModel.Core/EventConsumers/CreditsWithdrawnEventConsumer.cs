using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;
using Users.DomainEvents;

namespace ReadModel.Core.EventConsumers
{
    public class CreditsWithdrawnEventConsumer : EventConsumer<CreditsWithdrawn, CreditsWithdrawnEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public CreditsWithdrawnEventConsumer(ILogger<CreditsWithdrawnEventConsumer> logger, EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<CreditsWithdrawn> appEvent)
        {
            var filterBuilder = Builders<UserRead>.Filter;
            var userIdEq = filterBuilder
                .Eq(u => u.UserIdentity.UserId, appEvent.Event.UserId.ToString());
            var versionFilter = filterBuilder.Lt(f => f.Version, appEvent.Event.AggVersion);
            var update = Builders<UserRead>.Update
                .Set(u => u.Credits, appEvent.Event.AccountBalance)
                .Set(u => u.Version, appEvent.Event.AggVersion);

            await _dbContext.UsersReadModel
                .WithWriteConcern(new WriteConcern(mode: "majority", journal: true))
                .UpdateOneAsync(filterBuilder.And(userIdEq, versionFilter), update);
        }
    }
}
