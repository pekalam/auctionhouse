using AuctionBids.Domain.Repositories;
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
    using Auctions.Application.Commands.BuyNow;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.Domain;
    using Auctions.Tests.Base.Domain.Services.Fakes;
    using Categories.Domain;
    using Chronicle.Integrations.SQLServer;
    using ChronicleEfCoreStorage;
    using Common.Application.Commands;
    using Common.Application.Commands.Attributes;
    using Common.Application.Events;
    using Common.Application.Mediator;
    using Core.Common.Domain;
    using Core.Query.EventHandlers;
    using IntegrationService.AuctionPaymentVerification;
    using Polly;
    using ReadModel.Core;
    using ReadModel.Core.Model;
    using ReadModel.Core.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Test.ReadModel.Base;
    using TestConfigurationAccessor;
    using UserPayments.Application;
    using UserPayments.Domain.Repositories;
    using UserPayments.Domain.Services;
    using Users.Application;
    using Users.Application.Commands.SignUp;
    using Users.Domain.Repositories;
    using Users.Tests.Base.Mocks;
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

            ChronicleEfCoreIntegrationInitializer.Initialize(ServiceProvider);
            RabbitMqInstaller.InitializeEventSubscriptions(ServiceProvider, assemblyNames.Select(n => Assembly.Load(n)).ToArray());
            EfCoreReadModelNotificationsInstaller.Initialize(ServiceProvider);
            RabbitMqInstaller.InitializeEventConsumers(ServiceProvider, typeof(ReadModelInstaller).Assembly);
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
                services.AddSingleton(TestConfig.Instance);

                var commandHandlerAssemblies = assemblyNames.Select(n => Assembly.Load(n)).ToArray();
                //missing query dependencies
                services.AddCommonCommandDependencies(commandHandlerAssemblies);
                services.AddTransient<EventConsumerDependencies>();
                AttributeStrategies.LoadQueryAttributeStrategies(commandHandlerAssemblies);
                //

                services.AddAuctionsModule();
                services.AddAuctionBidsModule();
                services.AddUserPaymentsModule();
                services.AddUsersModule();
                services.AddChronicleSQLServerStorage((sagaType) => sagaType switch
                {
                    nameof(BuyNowSaga) => typeof(BuyNowSaga),
                    nameof(CreateAuctionSaga) => typeof(CreateAuctionSaga),
                    nameof(SignUpSaga) => typeof(SignUpSaga),
                    _ => throw new NotImplementedException(),
                }, TestConfig.Instance.GetChronicleSQLServerStorageConnectionString());

                services.AddSingleton<IAuctionCreateSessionStore, InMemAuctionCreateSessionStore>();
                services.AddSingleton<IAuctionRepository, FakeAuctionRepository>();
                services.AddSingleton<IAuctionBidsRepository, InMemoryAuctionBidsRepository>();
                services.AddSingleton<IUserRepository, InMemoryUserRepository>();
                services.AddTransient<ICategoryNamesToTreeIdsConversion, ConvertCategoryNamesToRootToLeafIdsMock>();
                services.AddTransient(s => Mock.Of<ILogger<CreateAuctionCommandHandler>>());
                services.AddTransient<CreateAuctionService>();
                services.AddTransient<IAuctionEndScheduler, AuctionEndSchedulerMock>();
                services.AddTransient(s => Mock.Of<IAuctionImageRepository>());
                services.AddSingleton<IUserIdentityService, UserIdentityServiceMock>(s => _userIdentityService);

                services.AddTransient<IAuctionImageConversion>((s) => Mock.Of<IAuctionImageConversion>());

                services.AddCommandEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());
                services.AddQueryEfCoreReadModelNotifications(TestConfig.Instance, settings: TestConfig.Instance.GetEfCoreReadModelNotificaitonsOptions());

                services.AddTransient<IAuctionPaymentVerification, AuctionPaymentVerification>();
                services.AddTransient<IAuctionUnlockScheduler>(s => Mock.Of<IAuctionUnlockScheduler>());

                services.AddSingleton<IUserPaymentsRepository>(s => new InMemortUserPaymentsRepository());
                services.AddSingleton<IUserAuthenticationDataRepository>(s => new InMemUserAuthenticationDataRepository());

                services.AddLogging(c =>
                {
                    c.AddXUnit(_outputHelper);
                    c.SetMinimumLevel(LogLevel.Debug);
                });

                services.AddRabbitMq(rabbitMqSettings: TestConfig.Instance.GetRabbitMqSettings());

                services.AddTransient<IImplProvider>((p) => new ImplProviderMock(p));

                services.AddReadModel(TestConfig.Instance.GetReadModelSettings());
                services.AddEventConsumers(typeof(ReadModelInstaller));
                services.AddAutoMapper(typeof(Auctionhouse.Query.QueryMapperProfile).Assembly);
                services.AddCategoriesModule();
                services.AddXmlCategoryTreeStore(settings: TestConfig.Instance.GetXmlStoreSettings());


                services.AddTransient<IOutboxItemStore, InMemoryOutboxItemStore>();
                services.AddTransient<IOutboxItemFinder, InMemoryPostProcessOutboxItemService>();

                services.AddSingleton<IEventBus>(s => new InMemoryEventBusDecorator(s.GetRequiredService<RabbitMqEventBus>()));
                services.AddSingleton(Mock.Of<IBidRaisedNotifications>());
                
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
            var eventBusDecorator = (InMemoryEventBusDecorator)ServiceProvider.GetRequiredService<IEventBus>();
            if(eventBusDecorator._eventBus is RabbitMqEventBus rabbit)
            {
                rabbit.Dispose();
            }
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