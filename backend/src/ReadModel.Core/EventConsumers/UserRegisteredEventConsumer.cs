using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using ReadModel.Core.Model;
using Users.Domain.Events;

namespace ReadModel.Core.EventConsumers
{
    public class UserRegisteredEventConsumer : EventConsumer<UserRegistered, UserRegisteredEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<UserRegisteredEventConsumer> _logger;

        public UserRegisteredEventConsumer(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<UserRegisteredEventConsumer> logger, Lazy<ISagaNotifications> sagaNotifications, Lazy<IImmediateNotifications> immediateNotifications)
            : base(appEventBuilder, logger, sagaNotifications, immediateNotifications)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override void Consume(IAppEvent<UserRegistered> message)
        {
            UserRegistered ev = message.Event;

            var userReadModel = new UserRead();
            userReadModel.UserIdentity = new UserIdentityRead(ev.UserId, ev.Username);

            try
            {
                _dbContext.UsersReadModel.InsertOne(userReadModel);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
