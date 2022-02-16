using Core.Query.EventHandlers;
using System.Reflection;


namespace RabbitMq.EventBus
{
    internal class QueryHandlerSeeker : IHandlerSeeker
    {
        public IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EventConsumer<,>))
                .Select(t =>
                {
                    return (t, t.BaseType.GenericTypeArguments[0]);
                });
        }
    }
}