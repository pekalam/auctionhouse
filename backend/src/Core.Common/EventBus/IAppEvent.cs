using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public interface IAppEvent<out T> where T : Event
    {
        CommandContext CommandContext { get; }
        T Event { get; }
    }
}