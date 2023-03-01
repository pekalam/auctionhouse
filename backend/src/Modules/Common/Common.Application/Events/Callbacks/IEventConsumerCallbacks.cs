using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Common.Application.Events.Callbacks
{
    public interface IEventConsumerCallbacks
    {
        Task OnEventProcessed<T>(IAppEvent<T> msg, ILogger logger) where T : Event;
    }
}