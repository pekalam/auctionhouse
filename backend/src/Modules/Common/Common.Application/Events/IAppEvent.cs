using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{

    public interface IAppEvent<out T> where T : Event
    {
        int RedeliveryCount { get; }
        CommandContext CommandContext { get; }
        T Event { get; }
    }
}