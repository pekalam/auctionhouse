using System;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers.User
{
    public class UserRegisteredHandler : EventConsumer<UserRegistered>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly ILogger<UserRegisteredHandler> _logger;

        public UserRegisteredHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<UserRegisteredHandler> logger) : base(appEventBuilder)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override void Consume(IAppEvent<UserRegistered> message)
        {
            UserRegistered ev = message.Event;

            var userReadModel = new UserRead();
            userReadModel.UserIdentity = new UserIdentityRead(ev.UserIdentity);

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