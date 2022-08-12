using Common.Application.Events;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Query.EventHandlers
{
    public interface IEventConsumerCallbacks
    {
        Task OnEventProcessed<T>(IAppEvent<T> msg, ILogger logger) where T : Event;
    }
}