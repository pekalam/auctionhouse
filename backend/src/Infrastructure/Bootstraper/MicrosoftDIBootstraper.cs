using EasyNetQ.AutoSubscribe;
using Infrastructure.Auth;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestEase;
using System;
using System.Reflection;
using System.Threading;
using Core;
using Core.Command.SignUp;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.SchedulerService;
using Core.Query.Handlers;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.SingleAuction;
using Core.Query.Queries.User.UserData;
using Core.Query.ReadModel;
using Infrastructure.Adapters;
using Infrastructure.Adapters.Repositories.AuctionImage;
using Infrastructure.Adapters.Repositories.EventStore;
using Infrastructure.Adapters.Services;
using Infrastructure.Adapters.Services.EventBus;
using Infrastructure.Adapters.Services.SchedulerService;

namespace Infrastructure.Bootstraper
{
    public static class MicrosoftDIBootstraper
    {
        public static void Bootstrap<UserIdentityServiceImplT, AuctionCreateSessionServiceT>(
            IServiceCollection serviceCollection,
            EventStoreConnectionSettings eventStoreConnectionSettings,
            RabbitMqSettings rabbitMqSettings, MongoDbSettings mongoDbSettings,
            TimeTaskServiceSettings timeTaskServiceSettings, CategoryNameServiceSettings categoryNameServiceSettings, ImageDbSettings imageDbSettings)
            where UserIdentityServiceImplT : class, IUserIdentityService
            where AuctionCreateSessionServiceT : class, IAuctionCreateSessionService
        {
            UsertAuthDbContext.Seed = false;

            serviceCollection.AddSingleton(eventStoreConnectionSettings);
            serviceCollection.AddSingleton(rabbitMqSettings);
            serviceCollection.AddSingleton(mongoDbSettings);
            serviceCollection.AddSingleton(categoryNameServiceSettings);
            serviceCollection.AddSingleton(timeTaskServiceSettings);
            serviceCollection.AddSingleton(imageDbSettings);

            serviceCollection.AddSingleton<IImplProvider, MicrosoftDIImplProvider>(provider =>
                new MicrosoftDIImplProvider(provider));
            serviceCollection.AddDbContext<UsertAuthDbContext>(builder => builder.UseInMemoryDatabase("infra"));
            serviceCollection.AddScoped<IUserAuthenticationDataRepository, UserAuthenticationDataRepository>();
            serviceCollection.AddSingleton<IUserIdentityService, UserIdentityServiceImplT>();
            serviceCollection.AddScoped<IAuctionCreateSessionService, AuctionCreateSessionServiceT>();

            serviceCollection.AddSingleton<IAppEventBuilder, AppEventRabbitMQBuilder>();
            serviceCollection.AddSingleton<IEventBus, RabbitMqEventBus>();
            serviceCollection.AddSingleton<ICategoryTreeService, CategoryTreeService>();
            serviceCollection.AddSingleton<CategoryBuilder>();

            serviceCollection.AddScoped<AuctionCreatedHandler>();
            serviceCollection.AddScoped<AuctionRaisedHandler>();
            serviceCollection.AddScoped<UserRegisteredHandler>();
            serviceCollection.AddScoped<AuctionImageAddedHandler>();
            serviceCollection.AddScoped<AuctionCompletedHandler>();

            serviceCollection.AddScoped<AuctionsQueryHandler>();
            serviceCollection.AddScoped<UserDataQueryHandler>();
            serviceCollection.AddScoped<AuctionQueryHandler>();


            serviceCollection.AddSingleton<ReadModelDbContext>();
            serviceCollection.AddSingleton<ImageDbContext>();

            serviceCollection.AddScoped<IAuctionImageRepository, AuctionImageRepository>();
            serviceCollection.AddSingleton<IAuctionImageSizeConverterService, AuctionImageSizeConverterService>();
            

            serviceCollection.AddScoped<EventBusService>();

            serviceCollection.AddSingleton<ESConnectionContext>();
            serviceCollection.AddScoped<IAuctionRepository, ESAuctionRepository>();
            serviceCollection.AddScoped<IUserRepository, ESUserRepository>();
            serviceCollection.AddSingleton<ITimeTaskClient>(provider =>
                {
                    var client = RestClient.For<ITimeTaskClient>(timeTaskServiceSettings.ConnectionString);
                    client.ApiKey = timeTaskServiceSettings.ApiKey;
                    return client;
                }
            );
            serviceCollection.AddSingleton<IAuctionSchedulerService, AuctionSchedulerService>();

            serviceCollection.AddMediatR(Assembly.Load("Core.Command"), Assembly.Load("Core.Query"));
        }

        public static void Init(IServiceProvider serviceProvider)
        {
            RollbackHandlerRegistry.ImplProvider = serviceProvider.GetRequiredService<IImplProvider>();
            var rabbitmq = (RabbitMqEventBus) serviceProvider.GetRequiredService<IEventBus>();
            rabbitmq.Init(serviceProvider.GetRequiredService<AuctionCreatedHandler>(),
                serviceProvider.GetRequiredService<AuctionRaisedHandler>(),
                serviceProvider.GetRequiredService<UserRegisteredHandler>(),
                serviceProvider.GetRequiredService<AuctionImageAddedHandler>(),
                serviceProvider.GetRequiredService<AuctionCompletedHandler>());

            var esconnection = serviceProvider.GetRequiredService<ESConnectionContext>();
            esconnection.Connect();

            ((CategoryTreeService) (serviceProvider.GetRequiredService<ICategoryTreeService>())).Init();

            if (UsertAuthDbContext.Seed)
            {
                serviceProvider.GetRequiredService<UsertAuthDbContext>().Database.EnsureCreated();
            }

            serviceProvider.GetRequiredService<IMediator>()
                .Send(new SignUpCommand("test", "pass", new CorrelationId("123")), CancellationToken.None);
        }
    }
}