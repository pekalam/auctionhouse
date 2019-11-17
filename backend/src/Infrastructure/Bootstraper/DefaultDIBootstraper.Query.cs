using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain;
using Core.Common.Domain.Auctions;
using Core.Common.Domain.Categories;
using Core.Common.EventBus;
using Core.Query.Handlers;
using Core.Query.ReadModel;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;

namespace Infrastructure.Bootstraper
{
    //TODO
    public static partial class DefaultDIBootstraper
    {
        public static class Query
        {
            private static void ConfigureEventBus(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<IAppEventBuilder, AppEventRabbitMQBuilder>();
                serviceCollection.AddSingleton<IEventBus, RabbitMqEventBus>();
                serviceCollection.AddScoped<EventBusService>();
            }

            private static void ConfigureEventHandlers(IServiceCollection serviceCollection)
            {
                serviceCollection.AddScoped<AuctionCreatedHandler>();
                serviceCollection.AddScoped<AuctionRaisedHandler>();
                serviceCollection.AddScoped<UserRegisteredHandler>();
                serviceCollection.AddScoped<AuctionImageAddedHandler>();
                serviceCollection.AddScoped<AuctionCompletedHandler>();
            }

            private static void ConfigureImageServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ImageDbContext>();
                serviceCollection.AddScoped<IAuctionImageRepository, AuctionImageRepository>();
                serviceCollection.AddSingleton<IAuctionImageSizeConverterService, AuctionImageSizeConverterService>();
            }

            private static void ConfigureCategoryService(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ICategoryTreeService, CategoryTreeService>();
                serviceCollection.AddSingleton<CategoryBuilder>();
            }

            private static void ConfigureSettings(IServiceCollection serviceCollection, MongoDbSettings mongoDbSettings,
                CategoryNameServiceSettings categoryNameServiceSettings, ImageDbSettings imageDbSettings,
                RabbitMqSettings rabbitMqSettings)
            {
                serviceCollection.AddSingleton(rabbitMqSettings);
                serviceCollection.AddSingleton(mongoDbSettings);
                serviceCollection.AddSingleton(categoryNameServiceSettings);
                serviceCollection.AddSingleton(imageDbSettings);
            }

            public static void Configure(IServiceCollection serviceCollection,
                MongoDbSettings mongoDbSettings,
                CategoryNameServiceSettings categoryNameServiceSettings,
                ImageDbSettings imageDbSettings,
                RabbitMqSettings rabbitMqSettings)
            {
                serviceCollection.AddSingleton<ReadModelDbContext>();

                ConfigureSettings(serviceCollection, mongoDbSettings, categoryNameServiceSettings, imageDbSettings, rabbitMqSettings);
                //ConfigureCategoryService(serviceCollection);
                //ConfigureImageServices(serviceCollection);
                ConfigureEventBus(serviceCollection);
                ConfigureEventHandlers(serviceCollection);

                serviceCollection.AddMediatR(Assembly.Load("Core.Command"), Assembly.Load("Core.Query"));

                serviceCollection.AddSingleton<IImplProvider, DefaultDIImplProvider>(provider =>
                    new DefaultDIImplProvider(provider));
            }

            public static void Start(IServiceProvider serviceProvider)
            {
                var implProvider = serviceProvider.GetRequiredService<IImplProvider>();
                RollbackHandlerRegistry.ImplProvider = implProvider;
                var rabbitmq = (RabbitMqEventBus) implProvider.Get<IEventBus>();
                rabbitmq.InitSubscribers(
                    new RabbitMqEventConsumerFactory(() => implProvider.Get<AuctionCreatedHandler>(),
                        EventNames.AuctionCreatedEventName),
                    new RabbitMqEventConsumerFactory(() => implProvider.Get<AuctionRaisedHandler>(),
                        EventNames.AuctionRaisedEventName),
                    new RabbitMqEventConsumerFactory(() => implProvider.Get<UserRegisteredHandler>(),
                        EventNames.UserRegisteredEventName),
                    new RabbitMqEventConsumerFactory(
                        () => implProvider.Get<AuctionImageAddedHandler>(),
                        EventNames.AuctionImageAddedEventName),
                    new RabbitMqEventConsumerFactory(
                        () => implProvider.Get<AuctionCompletedHandler>(),
                        EventNames.AuctionCompletedEventName)
                );
            }
        }
    }
}