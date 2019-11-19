using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public interface IAppEvent<out T> where T : Event
    {
        ICommand Command { get; }
        CorrelationId CorrelationId { get; }
        T Event { get; }
    }
}