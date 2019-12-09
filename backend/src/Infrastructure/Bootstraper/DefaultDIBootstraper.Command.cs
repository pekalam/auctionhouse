using System;
using System.Linq;
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
using Core.Common.Command;
using Core.Common.DomainServices;
using Infrastructure.Auth;
using Infrastructure.Repositories.AuctionImage;
using Infrastructure.Repositories.SQLServer;
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
                IServiceCollection serviceCollection, MsSqlConnectionSettings sqlServerConnectionSettings,
                RabbitMqSettings rabbitMqSettings, TimeTaskServiceSettings timeTaskServiceSettings,
                ImageDbSettings imageDbSettings,
                UserAuthDbContextOptions userAuthDbContextOptions,
                CategoryNameServiceSettings categoryNameServiceSettings)
            {
                serviceCollection.AddSingleton(sqlServerConnectionSettings);
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
                serviceCollection.AddTransient<IAuctionCreateSessionService, AuctionCreateSessionServiceT>();
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

            private static void ConfigureImageServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ImageDbContext>();
                serviceCollection.AddScoped<IAuctionImageRepository, AuctionImageRepository>();
                serviceCollection.AddSingleton<IAuctionImageSizeConverterService, AuctionImageSizeConverterService>();
                serviceCollection.AddScoped<AuctionImageService>();
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
                serviceCollection.AddScoped<IAuctionRepository, MsSqlAuctionRepository>();
                serviceCollection.AddScoped<IUserRepository, MsSqlUserRepository>();
            }


            private static void ConfigureDecoratedCommandHandlers(IServiceCollection serviceCollection)
            {
                var decoratedHandlerTypes = Assembly.Load("Core.Command")
                    .GetTypes()
                    .Where(type =>
                        type.BaseType != null && type.BaseType.IsGenericType &&
                        type.BaseType.GetGenericTypeDefinition() == typeof(DecoratedCommandHandlerBase<>));

                foreach (var handlerType in decoratedHandlerTypes)
                {
                    serviceCollection.AddScoped(handlerType);
                }
            }

            public static void Configure<UserIdentityServiceImplT, AuctionCreateSessionServiceImplT>(
                IServiceCollection serviceCollection,
                MsSqlConnectionSettings eventStoreConnectionSettings,
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
                ConfigureImageServices(serviceCollection);
                ConfigureDomainRepositories(serviceCollection);
                ConfigureAuctionShedulerService(serviceCollection, timeTaskServiceSettings);
                ConfigureDecoratedCommandHandlers(serviceCollection);
                serviceCollection.AddScoped<CreateAuctionCommandHandlerDepedencies>();


                serviceCollection.AddScoped<QueuedCommandHandler>();
                serviceCollection.AddScoped<MediatRCommandHandlerMediator>();
                serviceCollection.AddScoped<EventBusCmdHandlerMediator>();
                serviceCollection.AddScoped<ImmediateCommandMediator>();
                serviceCollection.AddScoped<QueuedCommandMediator>();
            }


        }
    }
}