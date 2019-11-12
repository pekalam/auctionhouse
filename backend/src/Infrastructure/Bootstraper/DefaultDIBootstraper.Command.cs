using System;
using System.Reflection;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.Domain.Users;
using Core.Common.EventBus;
using Core.Common.SchedulerService;
using Core.Command.CreateAuction;
using Infrastructure.Auth;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Repositories.EventStore;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;
using Infrastructure.Services.SchedulerService;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestEase;

namespace Infrastructure.Bootstraper
{
    public static partial class DefaultDIBootstraper
    {
        public static class Command
        {
            private static void ConfigureServiceSettings(
                IServiceCollection serviceCollection, EventStoreConnectionSettings eventStoreConnectionSettings,
                RabbitMqSettings rabbitMqSettings, TimeTaskServiceSettings timeTaskServiceSettings,
                ImageDbSettings imageDbSettings,
                UserAuthDbContextOptions userAuthDbContextOptions,
                CategoryNameServiceSettings categoryNameServiceSettings)
            {
                serviceCollection.AddSingleton(eventStoreConnectionSettings);
                serviceCollection.AddSingleton(rabbitMqSettings);
                serviceCollection.AddSingleton(categoryNameServiceSettings);
                serviceCollection.AddSingleton(timeTaskServiceSettings);
                serviceCollection.AddSingleton(imageDbSettings);
                serviceCollection.AddSingleton(userAuthDbContextOptions);
            }

            private static void
                ConfigureAuctionCreateSessionService<AuctionCreateSessionServiceT>(
                    IServiceCollection serviceCollection)
                where AuctionCreateSessionServiceT : class, IAuctionCreateSessionService
            {
                serviceCollection.AddScoped<IAuctionCreateSessionService, AuctionCreateSessionServiceT>();
            }

            private static void
                ConfigureUserIdentitySessionService<UserIdentityServiceImplT>(IServiceCollection serviceCollection)
                where UserIdentityServiceImplT : class, IUserIdentityService
            {
                serviceCollection.AddSingleton<IUserIdentityService, UserIdentityServiceImplT>();
            }

            private static void ConfigureAuthDbServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<UsertAuthDbContext>();
                serviceCollection.AddScoped<IUserAuthenticationDataRepository, UserAuthenticationDataRepository>();
            }

            private static void ConfigureEventBus(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<IAppEventBuilder, AppEventRabbitMQBuilder>();
                serviceCollection.AddSingleton<IEventBus, RabbitMqEventBus>();
                serviceCollection.AddScoped<EventBusService>();
            }

            private static void ConfigureMediatR(IServiceCollection serviceCollection)
            {
                serviceCollection.AddMediatR(Assembly.Load("Core.Command"));
            }

            private static void ConfigureImageServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ImageDbContext>();
                serviceCollection.AddScoped<IAuctionImageRepository, AuctionImageRepository>();
                serviceCollection.AddSingleton<IAuctionImageSizeConverterService, AuctionImageSizeConverterService>();
            }

            private static void ConfigureCategoryServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ICategoryTreeService, CategoryTreeService>();
                serviceCollection.AddSingleton<CategoryBuilder>();
            }

            private static void ConfigureAuctionShedulerService(IServiceCollection serviceCollection,
                TimeTaskServiceSettings timeTaskServiceSettings)
            {
                serviceCollection.AddSingleton<ITimeTaskClient>(provider =>
                    {
                        var client = RestClient.For<ITimeTaskClient>(timeTaskServiceSettings.ConnectionString);
                        client.ApiKey = timeTaskServiceSettings.ApiKey;
                        return client;
                    }
                );
                serviceCollection.AddSingleton<IAuctionSchedulerService, AuctionSchedulerService>();
            }

            private static void ConfigureDomainRepositories(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ESConnectionContext>();
                serviceCollection.AddScoped<IAuctionRepository, ESAuctionRepository>();
                serviceCollection.AddScoped<IUserRepository, ESUserRepository>();
            }

            public static void Configure<UserIdentityServiceImplT, AuctionCreateSessionServiceImplT>(
                IServiceCollection serviceCollection,
                EventStoreConnectionSettings eventStoreConnectionSettings,
                RabbitMqSettings rabbitMqSettings,
                TimeTaskServiceSettings timeTaskServiceSettings,
                ImageDbSettings imageDbSettings,
                UserAuthDbContextOptions userAuthDbContextOptions,
                CategoryNameServiceSettings categoryNameServiceSettings
            )
                where UserIdentityServiceImplT : class, IUserIdentityService
                where AuctionCreateSessionServiceImplT : class, IAuctionCreateSessionService
            {


                ConfigureServiceSettings(serviceCollection, eventStoreConnectionSettings, rabbitMqSettings,
                    timeTaskServiceSettings, imageDbSettings, userAuthDbContextOptions, categoryNameServiceSettings);
                ConfigureAuthDbServices(serviceCollection);
                ConfigureUserIdentitySessionService<UserIdentityServiceImplT>(serviceCollection);
                ConfigureAuctionCreateSessionService<AuctionCreateSessionServiceImplT>(serviceCollection);
                ConfigureCategoryServices(serviceCollection);
                ConfigureImageServices(serviceCollection);
                //ConfigureEventBus(serviceCollection);
                ConfigureDomainRepositories(serviceCollection);
                ConfigureAuctionShedulerService(serviceCollection, timeTaskServiceSettings);
                //ConfigureMediatR(serviceCollection);

//                serviceCollection.AddSingleton<IImplProvider, DefaultDIImplProvider>(provider =>
//                    new DefaultDIImplProvider(provider));
				serviceCollection.AddScoped<CreateAuctionCommandHandlerDepedencies>();
            }

            public static void Start(IServiceProvider serviceProvider)
            {
                var esconnection = serviceProvider.GetRequiredService<ESConnectionContext>();
                esconnection.Connect();

                ((CategoryTreeService) (serviceProvider.GetRequiredService<ICategoryTreeService>())).Init();
            }
        }
    }
}