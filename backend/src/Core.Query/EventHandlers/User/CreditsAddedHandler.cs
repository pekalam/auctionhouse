using System;
using System.Collections.Generic;
using System.Text;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using Core.Common.RequestStatusSender;
using Core.Query.Exceptions;
using Core.Query.ReadModel;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Core.Query.EventHandlers.User
{
    internal class CreditsChangeHelper
    {
        public static UserRead AddCredits<T>(ReadModelDbContext dbContext, IAppEvent<T> appEvent,
            decimal creditsCount, UserId userId,
            IRequestStatusSender requestStatusService) where T : Event
        {
            var filter = Builders<UserRead>.Filter.Eq(read => read.UserIdentity.UserId,
                userId.ToString());

            var update = Builders<UserRead>.Update.Inc(read => read.Credits, creditsCount);

            var userRead = dbContext.UsersReadModel.FindOneAndUpdate(filter, update);
            if (userRead == null)
            {
                requestStatusService.TrySendRequestFailureToUser(appEvent, userId);
                throw new QueryException("Null userReadModel");
            }

            requestStatusService.TrySendReqestCompletionToUser(appEvent, userId,
                new Dictionary<string, object>()
                {
                    {"ammount", creditsCount}
                });
            return userRead;
        }
    }

    public class CreditsAddedHandler : EventConsumer<CreditsAdded>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IRequestStatusSender _requestStatusService;

        public CreditsAddedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<CreditsAddedHandler> logger, IRequestStatusSender requestStatusService) : base(appEventBuilder,
            logger)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
        }

        public override void Consume(IAppEvent<CreditsAdded> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent, appEvent.Event.CreditsCount,
                appEvent.Event.UserIdentity, _requestStatusService);
        }
    }

    public class CreditsReturnedHandler : EventConsumer<CreditsReturned>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IRequestStatusSender _requestStatusService;


        public CreditsReturnedHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<CreditsReturnedHandler> logger, IRequestStatusSender requestStatusService) : base(appEventBuilder,
            logger)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
        }

        public override void Consume(IAppEvent<CreditsReturned> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent, appEvent.Event.CreditsCount,
                appEvent.Event.UserIdentity, _requestStatusService);
        }
    }

    public class CreditsWithdrawnHandler : EventConsumer<CreditsWithdrawn>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IRequestStatusSender _requestStatusService;


        public CreditsWithdrawnHandler(IAppEventBuilder appEventBuilder, ReadModelDbContext dbContext,
            ILogger<CreditsWithdrawnHandler> logger, IRequestStatusSender requestStatusService) : base(appEventBuilder,
            logger)
        {
            _dbContext = dbContext;
            _requestStatusService = requestStatusService;
        }

        public override void Consume(IAppEvent<CreditsWithdrawn> appEvent)
        {
            CreditsChangeHelper.AddCredits(_dbContext, appEvent, -appEvent.Event.CreditsCount,
                appEvent.Event.UserIdentity, _requestStatusService);
        }
    }
}