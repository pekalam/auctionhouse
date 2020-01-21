using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public interface IAppEvent<out T> where T : Event
    {
        CommandBase CommandBase { get; }
        CorrelationId CorrelationId { get; }
        T Event { get; }
    }
}