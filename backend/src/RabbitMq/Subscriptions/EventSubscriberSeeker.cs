using Common.Application.Events;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal class EventSubscriberSeeker : IHandlerSeeker
    {
        public IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EventSubscriber<>))
                .Select(t =>
                {
                    return (t, t.BaseType.GenericTypeArguments[0]);
                });
        }
    }
}