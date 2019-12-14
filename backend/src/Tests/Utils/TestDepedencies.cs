using System;
using System.Linq;
using Core.Common.ApplicationServices;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.EventBus;
using Core.Query.ReadModel;
using EasyNetQ.Logging;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Repositories.SQLServer;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;
using Infrastructure.Services.SchedulerService;
using Infrastructure.Tests.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RestEase;

namespace FunctionalTests.Utils
{
	public class TestContextUtils
	{
		public static string GetParameterOrDefault(string key, string defaultValue)
		{
			return TestContext.Parameters.Exists(key) ? TestContext.Parameters[key] : defaultValue;
		}
	}

	class TestDepedencies
	{
		public IAuctionRepository AuctionRepository { get; private set; }
		public EventBusService EventBus { get; private set; }
		public ReadModelDbContext DbContext { get; private set; }
		public ITimeTaskClient TimeTaskClient { get; private set; }
		public IAppEventBuilder AppEventBuilder { get; } = new AppEventRabbitMQBuilder();
		public CategoryTreeService CategoryTreeService { get; private set; }
		public AuctionSchedulerService SchedulerService { get; private set; }
		public TimeTaskServiceSettings TimetaskServiceSettings { get; private set; }
		public IAuctionImageRepository AuctionImageRepository { get; private set; }

		public static Lazy<TestDepedencies> Instance = new Lazy<TestDepedencies>(() => new TestDepedencies());

		private RabbitMqEventBus _rabbitMqEventBus;

		public IAuctionCreateSessionService GetAuctionCreateSessionService(AuctionCreateSession session)
		{
			var service = new Mock<IAuctionCreateSessionService>();
			service.Setup(f => f.GetExistingSession())
				.Returns(session);
			service.Setup(f => f.RemoveSession());
			return service.Object;
		}

		private TestDepedencies()
		{
			LogProvider.SetCurrentLogProvider(new TestLogger());
			SetupCategoryTreeService();
			SetupAuctionRepository();
			SetupReadModelDbContext();
			SetupTimeTaskClient();
			SetupAuctionImageRepository();
		}

		private void SetupAuctionRepository()
		{
			var mssqlConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver", "Data Source=.;Initial Catalog=es;Integrated Security=True;");
			AuctionRepository = new MsSqlAuctionRepository(new MsSqlConnectionSettings()
			{
				ConnectionString = mssqlConnectionString
			});
		}

		private void SetupAuctionImageRepository()
		{
			var mongoDbConnectionString =
				TestContextUtils.GetParameterOrDefault("mongodb-connection-string", "mongodb://localhost:27017");
			var dbContext = new ImageDbContext(new ImageDbSettings()
			{
				ConnectionString = mongoDbConnectionString,
				DatabaseName = "appDb"
			});

			AuctionImageRepository = new AuctionImageRepository(dbContext, Mock.Of<ILogger<AuctionImageRepository>>());
		}

		public void SetupEventBus(
			params IEventConsumer[] eventHandlers)
		{
			var rabbitMqConnectionString =
				TestContextUtils.GetParameterOrDefault("rabbitmq-connection-string", "host=localhost");
			_rabbitMqEventBus = new RabbitMqEventBus(new RabbitMqSettings()
			{
				ConnectionString = rabbitMqConnectionString
			}, Mock.Of<ILogger<RabbitMqEventBus>>());
			_rabbitMqEventBus.InitSubscribers(eventHandlers.Select(h => new RabbitMqEventConsumerFactory(() => h, h.MessageType.Name)).ToArray());
			EventBus = new EventBusService(_rabbitMqEventBus ,AppEventBuilder);
		}

		public void DisconnectEventBus()
		{
			_rabbitMqEventBus.CancelSubscriptions();
		}


		private void SetupCategoryTreeService()
		{
			CategoryTreeService = new CategoryTreeService(new CategoryNameServiceSettings()
			{
				CategoriesFilePath = "./queries_functional_test_categories.xml",
				SchemaFilePath = "./_Categories-xml-data/categories.xsd"
			});
			CategoryTreeService.Init();
		}

		private void SetupReadModelDbContext()
		{
			var mongoDbConnectionString =
				TestContextUtils.GetParameterOrDefault("mongodb-connection-string", "mongodb://localhost:27017");
			var dbContext = new ReadModelDbContext(new MongoDbSettings()
			{
				ConnectionString = mongoDbConnectionString,
				DatabaseName = "appDb"
			}, new CategoryBuilder(CategoryTreeService));
			DbContext = dbContext;
		}

		private void SetupTimeTaskClient()
		{
			var timeTaskConnectionString = TestContextUtils.GetParameterOrDefault("quartz-web-task-service", "http://localhost:5001");
			var auctionEchoTaskEndpoint = TestContextUtils.GetParameterOrDefault("quartz-web-task-target-endpoint", "http://host.docker.internal:9998/test");
			TimetaskServiceSettings = new TimeTaskServiceSettings()
			{
				ConnectionString = timeTaskConnectionString,
				AuctionEndEchoTaskEndpoint = auctionEchoTaskEndpoint,
				ApiKey = "testk"
			};
			TimeTaskClient = RestClient.For<ITimeTaskClient>(TimetaskServiceSettings.ConnectionString);
			SchedulerService = new AuctionSchedulerService(TimeTaskClient, TimetaskServiceSettings);
		}
	}
}