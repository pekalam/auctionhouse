using Common.Application;
using FunctionalTests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using RabbitMq.EventBus;
using System;
using System.Reflection;

namespace FunctionalTests.Commands
{
    using Adapter.EfCore.ReadModelNotifications;
    using Chronicle.Integrations.SQLServer;
    using Common.Application.Commands;
    using Common.Application.Events;
    using Common.Application.Mediator;
    using Core.Common.Domain;
    using Core.Common.Domain.Users;
    using Moq;
    using Polly;
    using ReadModel.Core;
    using ReadModel.Core.Model;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Test.ReadModel.Base;
    using Users.Domain.Repositories;
    using Users.Tests.Base;
    using XmlCategoryTreeStore;
    using Xunit;
    using Xunit.Abstractions;

    public partial class TestBase : IDisposable
    {
        private readonly string[] assemblyNames;
        private readonly Assembly[] assemblies;
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IUserIdentityService> _userIdentityService;
        private readonly ReadModelUserReadTestHelper _modelUserReadTestHelper = new();
        private User _signedInUser;

        public IServiceProvider ServiceProvider { get; }

        public CommandQueryMediator Mediator
        {
            get
            {
                return ServiceProvider.GetRequiredService<CommandQueryMediator>();
            }
        }

        public User SignedInUser => _signedInUser;

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
            _userIdentityService = new Mock<IUserIdentityService>();
            _outputHelper = outputHelper;
            this.assemblyNames = assemblyNames;
            this.assemblies = assemblyNames.Select(n => Assembly.Load(n)).ToArray();
            ServiceProvider = BuildConfiguredServiceProvider();

            ChronicleEfCoreIntegrationInitializer.Initialize(ServiceProvider);
            RabbitMqInstaller.InitializeEventSubscriptions(ServiceProvider, assemblyNames.Select(n => Assembly.Load(n)).ToArray());
            EfCoreReadModelNotificationsInstaller.Initialize(ServiceProvider);
            RabbitMqInstaller.InitializeEventConsumers(ServiceProvider, typeof(MongoDbReadModelInstaller).Assembly);
            XmlCategoryTreeStoreInstaller.Init(ServiceProvider);
            ReadModelDbContext = ServiceProvider.GetRequiredService<ReadModelDbContext>();

            ChangeSignedInUser(0);
        }

        [MemberNotNull(nameof(_signedInUser))]
        public void ChangeSignedInUser(decimal initialCredits, string? userName = null)
        {
            _signedInUser = new GivenUser()
                .WithInitialCredits(initialCredits)
                .WithUserName(userName)
                .LoggedIn(_userIdentityService)
                .Build(ServiceProvider.GetRequiredService<IUserRepository>());
            _modelUserReadTestHelper.TryInsertUserRead(_signedInUser.AggregateId, ReadModelDbContext);
        }

        public async Task<RequestStatus> SendCommand<T>(T command) where T : ICommand
        {
            using var scope = ServiceProvider.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<CommandQueryMediator>().Send(command);
            return result;
        }
    }
}