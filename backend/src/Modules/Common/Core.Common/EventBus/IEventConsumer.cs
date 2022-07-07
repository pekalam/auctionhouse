using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.FunctionalTests")]
namespace Core.Common.EventBus
{
    public interface IEventConsumer
    {
        Type MessageType { get; }
        void Dispatch(object message);
    }
}