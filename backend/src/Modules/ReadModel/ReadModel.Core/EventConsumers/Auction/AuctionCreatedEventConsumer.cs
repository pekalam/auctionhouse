﻿using Auctions.DomainEvents;
using AutoMapper;
using Common.Application.Events;
using Core.Common.Domain.Categories;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Contracts.Model;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionCreatedEventConsumer : EventConsumer<AuctionCreated, AuctionCreatedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly CategoryBuilder _categoryBuilder;

        public AuctionCreatedEventConsumer(ILogger<AuctionCreatedEventConsumer> logger, ReadModelDbContext dbContext, 
            IMapper mapper, CategoryBuilder categoryBuilder, EventConsumerDependencies dependencies) :
            base(logger, dependencies)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _categoryBuilder = categoryBuilder;
        }

        private Task<string?> GetUserName(Guid userId)
        {
            return _dbContext.UsersReadModel
                .Find(Builders<UserRead>.Filter.Eq(u => u.UserIdentity.UserId, userId.ToString()))
                .Project(u => u.UserIdentity.UserName)
                .SingleOrDefaultAsync()!;
        }

        public override async Task Consume(IAppEvent<AuctionCreated> appEvent)
        {
            var exists = (await _dbContext.AuctionsReadModel.Find(a => a.AuctionId == appEvent.Event.AuctionId.ToString()).FirstOrDefaultAsync()) != null;
            if (exists)
            {
                return;
            }

            var category = _categoryBuilder.FromCategoryIdList(appEvent.Event.Category.ToList());
            if (category is null)
            {
                throw new NullReferenceException("Invalid categories for auctionCreated event");
            }
            var categoryRead = CategoryRead.FromCategory(category);

            var userName = await GetUserName(appEvent.Event.Owner);
            var auctionRead = _mapper.Map<AuctionRead>(appEvent.Event);

            if (userName == null)
            {
                throw new InvalidOperationException();
            }
            auctionRead.Owner.UserName = userName;
            auctionRead.Category = categoryRead;

            

            _dbContext.AuctionsReadModel
                .WithWriteConcern(new WriteConcern(mode: "majority", journal: true))
                .InsertOne(auctionRead);
        }


    }
}
