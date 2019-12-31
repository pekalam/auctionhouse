using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers.User
{
    internal class CreditsChangeHelper
    {
        public static UserRead AddCredits(ReadModelDbContext dbContext, UserIdentity user, decimal credits)
        {
            var filter = Builders<UserRead>.Filter.Eq(read => read.UserIdentity.UserId,
                user.UserId.ToString());

            var update = Builders<UserRead>.Update.Inc(read => read.Credits, credits);

            var userRead = dbContext.UsersReadModel.FindOneAndUpdate(filter, update);
            if (userRead == null)
            {
                throw new QueryException("Null userReadModel");
            }

            return userRead;
        }
    }

    public class CreditsAddedHandler : EventConsumer<CreditsAdded>
    {
        private readonly ReadModelDbContext _dbContext;

        public CreditsAddedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, ILogger<CreditsAddedHandler> logger) : base(appEventBuilder, logger)
        {
            _dbContext = dbContext;
        }

        public override void Consume(IAppEvent<CreditsAdded> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent.Event.UserIdentity, appEvent.Event.CreditsCount);
        }
    }

    public class CreditsReturnedHandler : EventConsumer<CreditsReturned>
    {
        private readonly ReadModelDbContext _dbContext;

        public CreditsReturnedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, ILogger<CreditsReturnedHandler> logger) : base(appEventBuilder, logger)
        {
            _dbContext = dbContext;
        }

        public override void Consume(IAppEvent<CreditsReturned> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent.Event.UserIdentity, appEvent.Event.CreditsCount);
        }
    }

    public class CreditsWithdrawnHandler : EventConsumer<CreditsWithdrawn>
    {
        private readonly ReadModelDbContext _dbContext;

        public CreditsWithdrawnHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext, ILogger<CreditsWithdrawnHandler> logger) : base(appEventBuilder, logger)
        {
            _dbContext = dbContext;
        }

        public override void Consume(IAppEvent<CreditsWithdrawn> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent.Event.UserIdentity, -appEvent.Event.CreditsCount);
        }
    }
}
