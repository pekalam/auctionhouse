﻿using AuctionBids.Domain.Repositories;
using Auctions.Application;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.SagaNotifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMq.EventBus;
using System;
using System.Reflection;
using FunctionalTests.Mocks;

namespace FunctionalTests.Commands
{
    using Adapter.EfCore.ReadModelNotifications;
    using AuctionBids.Application;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.Domain;
    using Categories.Domain;
    using Common.Application.Commands;
    using Common.Application.Events;
    using Common.Application.Mediator;
    using Core.Common.Domain;
    using Polly;
    using ReadModel.Core;
    using ReadModel.Core.Model;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Test.Auctions.Base.Mocks;
    using Test.ReadModel.Base;
    using Test.Users.Base.Mocks;
    using UserPayments.Application;
    using UserPayments.Domain.Repositories;
    using Users.Application;
    using Users.Domain.Repositories;
    using XmlCategoryTreeStore;
    using Xunit;
    using Xunit.Abstractions;

    public class TestBase : IDisposable
    {
        private string[] assemblyNames;
        private ITestOutputHelper _outputHelper;
        private UserIdentityServiceMock _userIdentityService;
        private ReadModelUserReadTestHelper _modelUserReadTestHelper = new();

        public IServiceProvider ServiceProvider { get; }

        public ImmediateCommandQueryMediator Mediator
        {
            get
            {
                return ServiceProvider.GetRequiredService<ImmediateCommandQueryMediator>();
            }
        }

        public UserId CurrentUser { get; } = UserId.New();

        public ReadModelDbContext ReadModelDbContext { get; }

        internal SagaEventsConfirmationDbContext SagaEventsConfirmationDbContext
        {
            get
            {
                return ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            }
        }

        public IReadOnlyList<IAppEvent<Event>> SentEvents => InMemoryEventBusDecorator.SentEvents;

        public TestBase(ITestOutputHelper outputHelper, params string[] assemblyNames)
        {
            _outputHelper = outputHelper;
            var userId = Guid.NewGuid();
            _userIdentityService = new(userId);
            this.assemblyNames = assemblyNames;
            ServiceProvider = BuildConfiguredServiceProvider();

            RabbitMqInstaller.InitializeEventSubscriptions(ServiceProvider, assemblyNames.Select(n => Assembly.Load(n)).ToArray());
            CommonInstaller.InitAttributeStrategies(assemblyNames);
            EfCoreReadModelNotificationsInstaller.Initialize(ServiceProvider);
            ReadModelInstaller.InitSubscribers(ServiceProvider);
            XmlCategoryTreeStoreInstaller.Init(ServiceProvider);
            ReadModelDbContext = ServiceProvider.GetRequiredService<ReadModelDbContext>();

            _modelUserReadTestHelper.TryInsertUserRead(userId, ReadModelDbContext);
        }

        public void ChangeSignedInUser(Guid userId)
        {
            _userIdentityService.UserId = userId;
            _modelUserReadTestHelper.TryInsertUserRead(userId, ReadModelDbContext);
        }

        public async Task<RequestStatus> SendCommand<T>(T command) where T : ICommand
        {
            using var scope = ServiceProvider.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<ImmediateCommandQueryMediator>().Send(command);
            return result;
        }

        protected void TruncateReadModelNotificaitons(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmations = dbContext.SagaEventsConfirmations.ToList();
            dbContext.SagaEventsConfirmations.RemoveRange(confirmations);

            var eventsToConfirm = dbContext.SagaEventsToConfirm.ToList();
            dbContext.SagaEventsToConfirm.RemoveRange(eventsToConfirm);

            dbContext.SaveChanges();
        }

        protected virtual void AddServices(IServiceCollection services)
        {

        }

        protected (bool sagaCompleted, bool allEventsProcessed) SagaShouldBeCompletedAndAllEventsShouldBeProcessed(RequestStatus requestStatus)
        {
            var eventConfirmations = SagaEventsConfirmationDbContext.SagaEventsConfirmations.FirstOrDefault(e => e.CommandId == requestStatus.CommandId.Id);
            var sagaCompleted = eventConfirmations?.Completed == true;
            var allEventsProcessed = false;
            if (eventConfirmations != null)
            {
                var eventsToProcess = SagaEventsConfirmationDbContext.SagaEventsToConfirm
                 .Where(e => e.CorrelationId == eventConfirmations.CorrelationId).ToList();

                allEventsProcessed = eventsToProcess.Count > 0 && eventsToProcess.All(e => e.Processed);
            }

            return (sagaCompleted, allEventsProcessed);
        }

        private IServiceProvider BuildConfiguredServiceProvider()
        {
            return DiTestUtils.CreateServiceProvider((services) =>
            {
                services.AddCommon(assemblyNames.Select(n => Assembly.Load(n)).ToArray());
                services.AddAuctionsModule(assemblyNames);
                services.AddAuctionBidsModule();
                services.AddUserPaymentsModule();
                services.AddUsersModule();

                services.AddSingleton<IAuctionCreateSessionStore, InMemAuctionCreateSessionStore>();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
                services.AddSingleton<IAuctionBidsRepository, InMemoryAuctionBidsRepository>();
                services.AddSingleton<IUserRepository, InMemoryUserRepository>();
                services.AddTransient<IConvertCategoryNamesToRootToLeafIds, ConvertCategoryNamesToRootToLeafIdsMock>();
                services.AddTransient(s => Mock.Of<ILogger<CreateAuctionCommandHandler>>());
                services.AddTransient<CreateAuctionService>();
                services.AddTransient<IAuctionEndScheduler, AuctionEndSchedulerMock>();
                services.AddTransient(s => Mock.Of<IAuctionImageRepository>());
                services.AddSingleton<IUserIdentityService, UserIdentityServiceMock>(s => _userIdentityService);

                services.AddTransient<IAuctionImageConversion>((s) => Mock.Of<IAuctionImageConversion>());

                services.AddEfCoreReadModelNotifications(new EfCoreReadModelNotificaitonsOptions
                {
                    Provider = "sqlserver",
                    ConnectionString = @"Data Source=127.0.0.1;Initial Catalog=AuctionhouseDatabase;MultipleActiveResultSets=True;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234;",
                });

                var paymentVerification = new Mock<IAuctionPaymentVerification>();
                paymentVerification
                .Setup(f => f.Verification(It.IsAny<Auction>(), It.IsAny<UserId>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
                services.AddTransient<IAuctionPaymentVerification>(s => paymentVerification.Object);
                services.AddTransient<IAuctionUnlockScheduler>(s => Mock.Of<IAuctionUnlockScheduler>());

                services.AddSingleton<IUserPaymentsRepository>(s => new InMemortUserPaymentsRepository());
                services.AddSingleton<IUserAuthenticationDataRepository>(s => new InMemUserAuthenticationDataRepository());

                services.AddLogging(c =>
                {
                    c.AddXUnit(_outputHelper);
                    c.SetMinimumLevel(LogLevel.Debug);
                });

                services.AddRabbitMq(new RabbitMqSettings
                {
                    ConnectionString = "host=localhost"
                });

                services.AddTransient<IImplProvider>((p) => new ImplProviderMock(p));

                services.AddReadModel(new MongoDbSettings
                {
                    ConnectionString = "mongodb://localhost:27017",
                    DatabaseName = "appDb"
                }, new RabbitMqSettings
                {
                    ConnectionString = "host=localhost",
                });
                services.AddEventConsumers(typeof(ReadModelInstaller));
                services.AddAutoMapper(typeof(Auctionhouse.Query.QueryMapperProfile).Assembly);
                services.AddCategoriesModule();
                services.AddXmlCategoryTreeStore(new XmlCategoryNameStoreSettings
                {
    CategoriesFilePath = "C:\\Users\\Marek\\source\\repos\\Csharp\\auctionhouse\\backend\\src\\Xml.CategoryTreeStore\\_Categories-xml-data\\categories.xml",
    SchemaFilePath = "C:\\Users\\Marek\\source\\repos\\Csharp\\auctionhouse\\backend\\src\\Xml.CategoryTreeStore\\_Categories-xml-data\\categories.xsd"
                });


                services.AddTransient<IOutboxItemStore, InMemoryOutboxItemStore>();
                services.AddTransient<IOutboxItemFinder, InMemoryPostProcessOutboxItemService>();

                services.AddSingleton<IEventBus>(s => new InMemoryEventBusDecorator(s.GetRequiredService<RabbitMqEventBus>()));


                AddServices(services);
            });
        }

        protected void AssertEventual(Func<bool> getResults)
        {
            var policy = Policy
              .HandleResult<bool>(p => p == false)
              .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
              );
            Assert.True(policy.Execute(getResults));
        }

        public virtual void Dispose()
        {
            _modelUserReadTestHelper.Dispose();
        }
    }

    public static class DiTestUtils
    {
        public static IServiceProvider CreateServiceProvider(Action<IServiceCollection> build)
        {
            var services = new ServiceCollection();
            build(services);
            return services.BuildServiceProvider();
        }
    }
}