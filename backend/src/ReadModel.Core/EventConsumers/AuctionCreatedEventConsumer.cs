﻿using Auctions.DomainEvents;
using AutoMapper;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using Core.Common.Domain.Categories;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ReadModel.Core.Model;

namespace ReadModel.Core.EventConsumers
{
    public class AuctionCreatedEventConsumer : EventConsumer<AuctionCreated, AuctionCreatedEventConsumer>
    {
        private readonly ReadModelDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly CategoryBuilder _categoryBuilder;

        public AuctionCreatedEventConsumer(IAppEventBuilder appEventBuilder, ILogger<AuctionCreatedEventConsumer> logger, Lazy<ISagaNotifications> sagaNotificationsFactory,
            Lazy<IImmediateNotifications> immediateNotifications, ReadModelDbContext dbContext, IMapper mapper, CategoryBuilder categoryBuilder) :
            base(appEventBuilder, logger, sagaNotificationsFactory, immediateNotifications)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _categoryBuilder = categoryBuilder;
        }

        public override void Consume(IAppEvent<AuctionCreated> appEvent)
        {
            var auctionRead = _mapper.Map<AuctionRead>(appEvent.Event);
            auctionRead.Owner.UserName = "test"; //TODO

            var category = _categoryBuilder.FromCategoryIdList(appEvent.Event.Category.ToList());
            if (category is null)
            {
                throw null;
            }

            auctionRead.Category = CreateCategoryRead(category);

            _dbContext.AuctionsReadModel.WithWriteConcern(new WriteConcern(mode: "majority", journal: true))
                .InsertOne(auctionRead);
        }

        private static CategoryRead CreateCategoryRead(Category? category)
        {
            var catRead = new CategoryRead();
            var currentCat = category;
            var currentCatRead = catRead;
            do
            {
                currentCatRead.Name = currentCat.Name;
                if (currentCat.SubCategory is not null)
                {
                    currentCatRead.SubCategory = new CategoryRead();
                }
                currentCatRead = currentCatRead.SubCategory;
                currentCat = currentCat.SubCategory;
            } while (currentCat is not null);
            return catRead;
        }
    }
}