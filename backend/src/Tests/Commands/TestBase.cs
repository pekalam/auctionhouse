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
    using Auctions.Application.Commands.StartAuctionCreateSession;
    using Auctions.Domain;
    using Categories.Domain;
    using Common.Application.Commands;
    using Common.Application.Mediator;
    using Polly;
    using ReadModel.Core;
    using ReadModel.Core.Model;
    using System.Linq;
    using System.Threading.Tasks;
    using UserPayments.Application;
    using UserPayments.Domain.Repositories;
    using XmlCategoryTreeStore;
    using Xunit;
    using Xunit.Abstractions;

    public class TestBase
    {
        private string[] assemblyNames;
        private ITestOutputHelper _outputHelper;
        private UserIdentityServiceMock _userIdentityService;

        public IServiceProvider ServiceProvider { get; }

        public ImmediateCommandQueryMediator Mediator { get; }

        public UserId CurrentUser { get; } = UserId.New();

        public TestBase(ITestOutputHelper outputHelper, params string[] assemblyNames)
        {
            _outputHelper = outputHelper;
            _userIdentityService = new(Guid.NewGuid());
            this.assemblyNames = assemblyNames;

            ServiceProvider = BuildConfiguredServiceProvider();
            RabbitMqInstaller.InitializeEventSubscriptions(ServiceProvider, assemblyNames.Select(n => Assembly.Load(n)).ToArray());
            CommonInstaller.InitAttributeStrategies(assemblyNames);
            EfCoreReadModelNotificationsInstaller.Initialize(ServiceProvider);
            ReadModelInstaller.InitSubscribers(ServiceProvider);
            XmlCategoryTreeStoreInstaller.Init(ServiceProvider);
            Mediator = ServiceProvider.GetRequiredService<ImmediateCommandQueryMediator>();
        }

        public void ChangeSignedInUser(Guid userId)
        {
            _userIdentityService.UserId = userId;
        }

        public Task<RequestStatus> SendCommand<T>(T command) where T : ICommand
        {
            return Mediator.Send(command);
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

        private IServiceProvider BuildConfiguredServiceProvider()
        {
            return DiTestUtils.CreateServiceProvider((services) =>
            {
                services.AddCommon(assemblyNames.Select(n => Assembly.Load(n)).ToArray());
                services.AddAuctionsModule(assemblyNames);
                services.AddAuctionBidsModule();
                services.AddUserPaymentsModule();

                services.AddSingleton<IAuctionCreateSessionStore, InMemAuctionCreateSessionStore>();
                services.AddSingleton<IAuctionRepository, InMemoryAuctionRepository>();
                services.AddSingleton<IAuctionBidsRepository, InMemoryAuctionBidsRepository>();
                services.AddTransient(s => () => s.GetRequiredService<IAuctionRepository>());
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
                    ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Marek\source\repos\Csharp\auctionhouse\backend\src\Tests\FunctionalTestsServer.mdf;Integrated Security=True",
                });

                var paymentVerification = new Mock<IAuctionPaymentVerification>();
                paymentVerification
                .Setup(f => f.Verification(It.IsAny<Auction>(), It.IsAny<UserId>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
                services.AddTransient<IAuctionPaymentVerification>(s => paymentVerification.Object);
                services.AddTransient<IAuctionUnlockScheduler>(s => Mock.Of<IAuctionUnlockScheduler>());

                services.AddSingleton<IUserPaymentsRepository>(s => new InMemortUserPaymentsRepository());

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