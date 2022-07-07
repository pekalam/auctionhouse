using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserPayments.Domain;

namespace ReadModel.Core.EventConsumers
{
    public class UserPaymentsCreatedEventConsumer : EventConsumer<UserPaymentsCreated, UserPaymentsCreatedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;

        public UserPaymentsCreatedEventConsumer(ILogger<UserPaymentsCreatedEventConsumer> logger, EventConsumerDependencies dependencies, ReadModelDbContext dbContext) : base(logger, dependencies)
        {
            _dbContext = dbContext;
        }

        public override async Task Consume(IAppEvent<UserPaymentsCreated> appEvent)
        {
            var userPaymentsRead = new UserPaymentsRead
            {
                UserId = appEvent.Event.UserId.ToString(),
            };
            var exists = (await _dbContext.UserPaymentsReadModel.Find(u => u.UserId == userPaymentsRead.UserId).FirstOrDefaultAsync()) != null;
            if (exists)
            {
                return;
            }

            await _dbContext.UserPaymentsReadModel.InsertOneAsync(userPaymentsRead);
        }
    }
}
