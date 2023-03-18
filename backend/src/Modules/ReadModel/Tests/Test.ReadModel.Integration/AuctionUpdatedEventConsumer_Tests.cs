using Auctions.DomainEvents;
using Auctions.DomainEvents.Update;
using Common.Application.Commands;
using Common.Application.Events;
using Common.Application.Events.Callbacks;
using Common.Tests.Base.Mocks.Events;
using Core.Common.Domain;
using Core.Common.Domain.Categories;
using Core.Query.EventHandlers;
using Core.Query.EventHandlers.AuctionUpdateHandlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestConfigurationAccessor;
using Xunit;

namespace ReadModel.Tests.Integration
{
    internal class FakeCategoryBuilder : CategoryBuilder //TODO 
    {
        public FakeCategoryBuilder() : base(null)
        {
        }

        public override Category FromCategoryIdList(List<int> categoryIds)
        {
            return new Category("", new CategoryId(1))
            {
                SubCategory = new Category("", new CategoryId(2))
                {
                    SubCategory = new Category("", new CategoryId(3))
                }
            };
        }

        public override Category FromCategoryNamesList(List<string> categoryNames)
        {
            return new Category("", new CategoryId(1))
            {
                SubCategory = new Category("", new CategoryId(2))
                {
                    SubCategory = new Category("", new CategoryId(3))
                }
            };
        }
    }

    [Trait("Category", "Integration")]
    public class AuctionUpdatedEventConsumer_Tests : IDisposable
    {
        ReadModelDbContext dbContext;
        AuctionRead auctionRead;
        AuctionUpdatedEventConsumer consumer;

        public AuctionUpdatedEventConsumer_Tests()
        {
            dbContext = new ReadModelDbContext(
                Options.Create(TestConfig.Instance.GetMongoDbSettings()));
            auctionRead = new AuctionRead
            {
                AuctionId = Guid.NewGuid().ToString(),
                Category = new CategoryRead
                {
                    Id = 1,
                    SubCategory = new CategoryRead
                    {
                        Id = 2,
                        SubCategory = new CategoryRead
                        {
                            Id = 3
                        }
                    }
                },
                Product = new ProductRead(),
                Tags = new string[] { "tag1", "tag2" }
            };
            dbContext.AuctionsReadModel.InsertOne(auctionRead);
            consumer = new AuctionUpdatedEventConsumer(
                Mock.Of<ILogger<AuctionUpdatedEventConsumer>>(),
                dbContext,
                new FakeCategoryBuilder(),
                GivenMockEventConsumerDependencies());
        }

        [Fact]
        public async Task Consumer_Should_Update_AuctionReadModel_In_DB()
        {
            const string expectedDescription = "x";
            const string expectedTag = "testTag";
            var appEvent = GivenAuctionUpdateEventGroup(
                    new AuctionDescriptionChanged(Guid.Parse(auctionRead.AuctionId), expectedDescription),
                    new AuctionTagsChanged(Guid.Parse(auctionRead.AuctionId), new string[] { expectedTag })
                );

            await consumer.Consume(appEvent);

            var dbAuction = dbContext.AuctionsReadModel.Find(a => a.AuctionId == auctionRead.AuctionId).First();
            dbAuction.Product.Description.Should().Be(expectedDescription);
            dbAuction.Tags.Should().BeEquivalentTo(new string[] { expectedTag });
        }

        private IAppEvent<AuctionUpdateEventGroup> GivenAuctionUpdateEventGroup(params UpdateEvent[] updateEvents)
        {
            var updateEvent = new AuctionUpdateEventGroup(Guid.NewGuid())
            {
                AggregateId = Guid.Parse(auctionRead.AuctionId),
                CategoryIds = new CategoryIds(auctionRead.Category.Id, auctionRead.Category.SubCategory.Id, auctionRead.Category.SubCategory.SubCategory.Id)
            };
            updateEvent.UpdateEvents.AddRange(updateEvents);
            return new TestAppEventBuilder()
                .WithCommandContext(CommandContext.CreateNew("test"))
                .WithEvent(updateEvent)
                .Build<AuctionUpdateEventGroup>();
        }

        private static EventConsumerDependencies GivenMockEventConsumerDependencies()
        {
            return new EventConsumerDependencies(new TestAppEventBuilder(), Mock.Of<IEventConsumerCallbacks>());
        }

        public void Dispose()
        {
            dbContext.AuctionsReadModel.DeleteMany(a => a.AuctionId == auctionRead.AuctionId);
        }
    }
}
