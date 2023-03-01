using Common.Application.Events;
using Core.Query.EventHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEventSubscribers(this IServiceCollection services, params Type[] typesFromAssembliesToScan)
        {
            if (typesFromAssembliesToScan.Length == 0) throw new ArgumentException($"Empty {nameof(typesFromAssembliesToScan)}");

            services.Scan(s =>
            {
                var x = s.FromAssembliesOf(typesFromAssembliesToScan)
                .AddClasses(filter =>
                {
                    filter.AssignableTo(typeof(EventSubscriber<>));
                }).AsSelf().WithTransientLifetime();
            });
        }

        public static void AddEventConsumers(this IServiceCollection services, params Type[] typesFromAssembliesToScan)
        {
            if (typesFromAssembliesToScan.Length == 0) throw new ArgumentException($"Empty {nameof(typesFromAssembliesToScan)}");

            services.Scan(s =>
            {
                var x = s.FromAssembliesOf(typesFromAssembliesToScan)
                .AddClasses(filter =>
                {
                    filter.AssignableTo(typeof(EventConsumer<,>));
                }).AsSelf().WithTransientLifetime();
            });
        }
    }
}
