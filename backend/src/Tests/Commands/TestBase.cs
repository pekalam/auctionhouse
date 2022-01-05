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
    using AuctionBids.Application;
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.Domain;
    using Common.Application.Commands;
    using Common.Application.Mediator;
    using Polly;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class TestBase
    {
        private string[] assemblyNames;

        public IServiceProvider ServiceProvider { get; }

        public ImmediateCommandMediator Mediator { get; }

        public UserId CurrentUser { get; } = UserId.New();

        public TestBase(params string[] assemblyNames)
        {
            this.assemblyNames = assemblyNames;

            ServiceProvider = BuildConfiguredServiceProvider();
            RabbitMqInstaller.InitializeEventSubscriptions(ServiceProvider, assemblyNames.Select(n => Assembly.Load(n)).ToArray());
            CommonInstaller.InitAttributeStrategies(assemblyNames);
            Mediator = ServiceProvider.GetRequiredService<ImmediateCommandMediator>();
        }

        public Task<RequestStatus> SendCommand<T>(T command) where T : ICommand
        {
            return Mediator.Send(command);
        }

        protected virtual void AddServices(IServiceCollection services)
        {

        }

        private IServiceProvider BuildConfiguredServiceProvider()
        {
            return DiTestUtils.CreateServiceProvider((services) =>
            {
                services.AddCommon(assemblyNames.Select(n => Assembly.Load(n)).ToArray());
                services.AddAuctionsModule(assemblyNames);
                services.AddAuctionBidsModule();

                services.AddSingleton<IAuctionCreateSessionStore, InMemAuctionCreateSessionStore>();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
                services.AddSingleton<IAuctionBidsRepository, InMemoryAuctionBidsRepository>();
                services.AddTransient(s => () => s.GetRequiredService<IAuctionRepository>());
                services.AddTransient<IConvertCategoryNamesToRootToLeafIds, ConvertCategoryNamesToRootToLeafIdsMock>();
                services.AddTransient(s => Mock.Of<ILogger<CreateAuctionCommandHandler>>());
                services.AddTransient<CreateAuctionService>();
                services.AddTransient<IAuctionEndScheduler, AuctionEndSchedulerMock>();
                services.AddTransient(s => Mock.Of<IAuctionImageRepository>());
                services.AddSingleton<IUserIdentityService, UserIdentityServiceMock>(s => new(CurrentUser.Value));

                services.AddTransient<IAuctionImageConversion>((s) => Mock.Of<IAuctionImageConversion>());
                services.AddTransient<ISagaNotifications, InMemorySagaNotifications>();

                services.AddLogging();

                services.AddRabbitMq(new RabbitMqSettings
                {
                    ConnectionString = "host=localhost"
                });

                services.AddTransient<IImplProvider>((p) => new ImplProviderMock(p));

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