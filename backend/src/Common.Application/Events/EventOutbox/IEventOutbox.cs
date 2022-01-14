using Common.Application.Commands;
using Core.Common.Domain;
using Microsoft.Extensions.Hosting;

namespace Common.Application.Events
{
    public interface IEventOutbox
    {
        Task SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode);
        Task SaveEvents(IEnumerable<Event> @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode);
    }
}
