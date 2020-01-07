using System;
using System.Reflection;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain.Categories;
using Core.Common.EventBus;
using Core.Common.Query;
using Core.Query;
using Core.Query.Mediator;
using Infrastructure.Services;
using Infrastructure.Services.EventBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Bootstraper
{
    public static partial class DefaultDIBootstraper
    {
        public static class Common
        {
            private static void ConfigureCategoryServices(IServiceCollection serviceCollection)
            {
                serviceCollection.AddSingleton<ICategoryTreeService, CategoryTreeService>();
                serviceCollection.AddSingleton<CategoryBuilder>();
            }

            private static void ConfigureEventBus(IServiceCollection serviceCollection)
            {
                serviceCollection.AddTransient<IAppEventBuilder, AppEventRabbitMQBuilder>();
                serviceCollection.AddSingleton<IEventBus, RabbitMqEventBus>();
                serviceCollection.AddScoped<EventBusService>();
            }

            public static void Configure(IServiceCollection serviceCollection)
            {
                ConfigureCategoryServices(serviceCollection);
                ConfigureEventBus(serviceCollection);

                
                serviceCollection.AddMediatR(
                    new Assembly[] {Assembly.Load("Core.Command"), Assembly.Load("Core.Query")},
                    configuration =>
                    {
                        configuration.AsTransient();
                    });


                serviceCollection.AddSingleton<IImplProvider, DefaultDIImplProvider>(provider =>
                    new DefaultDIImplProvider(provider.GetService<IServiceScopeFactory>()));

                serviceCollection.AddScoped<QueryMediator>();
            }

            public static void Start(IServiceProvider serviceProvider, Action<EventArgs, ILogger> eventBusDisconnectedCallback)
            {
                ((CategoryTreeService) (serviceProvider.GetRequiredService<ICategoryTreeService>())).Init();
                var implProvider = serviceProvider.GetRequiredService<IImplProvider>();
                RollbackHandlerRegistry.ImplProvider = implProvider;
                var rabbitmq = (RabbitMqEventBus)implProvider.Get<IEventBus>();
                rabbitmq.Disconnected += eventBusDisconnectedCallback;
                rabbitmq.InitSubscribers("Core.Query", implProvider);
                rabbitmq.InitCommandSubscribers("Core.Command", implProvider);
            }
        }
    }
}