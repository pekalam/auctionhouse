using System.Reflection;


namespace RabbitMq.EventBus
{
    internal interface IHandlerSeeker
    {
        IEnumerable<(Type handlerType, Type eventType)> Seek(params Assembly[] assemblies);
    }
}