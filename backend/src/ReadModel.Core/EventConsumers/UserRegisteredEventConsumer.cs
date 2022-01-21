using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using ReadModel.Core.Model;
using Users.Domain.Events;
using Users.DomainEvents;

namespace ReadModel.Core.EventConsumers
{
    public class UserRegisteredEventConsumer : EventConsumer<UserRegistered, UserRegisteredEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<UserRegisteredEventConsumer> _logger;

        public UserRegisteredEventConsumer(ReadModelDbContext dbContext, ILogger<UserRegisteredEventConsumer> logger, EventConsumerDependencies dependencies)
            : base(logger, dependencies)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override Task Consume(IAppEvent<UserRegistered> message)
        {
            UserRegistered ev = message.Event;

            var userReadModel = new UserRead
            {
                Credits = ev.InitialCredits,
                Version = ev.AggVersion,
            };
            userReadModel.UserIdentity = new UserIdentityRead(ev.UserId, ev.Username);

            _dbContext.UsersReadModel.InsertOne(userReadModel);
            return Task.CompletedTask;
        }
    }
}
