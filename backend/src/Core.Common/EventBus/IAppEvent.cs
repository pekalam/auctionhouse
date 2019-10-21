using Core.Common.Domain;
using Core.Common.Interfaces;

namespace Core.Common.EventBus
{
    public interface IAppEvent<out T> where T : Event
    {
        ICommand Command { get; }
        CorrelationId CorrelationId { get; }
        T Event { get; }
    }
}