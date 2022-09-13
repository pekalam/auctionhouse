using Common.Application;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.EventBus;
using System;
using System.Reflection;

namespace FunctionalTests.Commands
{
    using Adapter.EfCore.ReadModelNotifications;
    using Auctions.Domain;
    using Auctions.Tests.Base.Domain.Services.Fakes;
    using Chronicle.Integrations.SQLServer;
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
    using Test.ReadModel.Base;
    using Users.Tests.Base.Mocks;
    using XmlCategoryTreeStore;
    using Xunit;
    using Xunit.Abstractions;

    public partial class TestBase : IDisposable
    {
        private readonly string[] assemblyNames;
        private readonly ITestOutputHelper _outputHelper;
        private readonly UserIdentityServiceMock _userIdentityService;
        private readonly ReadModelUserReadTestHelper _modelUserReadTestHelper = new();

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

        internal (bool sagaCompleted, bool allEventsProcessed) SagaShouldBeCompletedAndAllEventsShouldBeProcessed(RequestStatus requestStatus)
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


        protected void AssertEventual(Func<bool> getResults)
        {
            var policy = Policy
              .HandleResult<bool>(p => p == false)
              .WaitAndRetry(5, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
              );
            Assert.True(policy.Execute(getResults));
        }

        private static void TruncateReadModelNotificaitons(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<SagaEventsConfirmationDbContext>();
            var confirmations = dbContext.SagaEventsConfirmations.ToList();
            dbContext.SagaEventsConfirmations.RemoveRange(confirmations);

            var eventsToConfirm = dbContext.SagaEventsToConfirm.ToList();
            dbContext.SagaEventsToConfirm.RemoveRange(eventsToConfirm);

            dbContext.SaveChanges();
        }

        public virtual void Dispose()
        {
            _modelUserReadTestHelper.Dispose();
            var eventBusDecorator = (InMemoryEventBusDecorator)ServiceProvider.GetRequiredService<IEventBus>();
            if (eventBusDecorator._eventBus is RabbitMqEventBus rabbit)
            {
                rabbit.Dispose();
            }
            InMemAuctionCreateSessionStore.Instance.RemoveSession();
            InMemoryAuctionBidsRepository.Instance.Clear();
            InMemoryUserRepository.Instance.Clear();
            FakeAuctionRepository.Instance.Clear();
            TruncateReadModelNotificaitons(ServiceProvider);
        }
    }
}