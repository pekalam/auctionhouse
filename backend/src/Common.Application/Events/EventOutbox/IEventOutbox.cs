using Common.Application.Commands;
using Core.Common.Domain;
using Microsoft.Extensions.Hosting;

namespace Common.Application.Events
{
    public interface IEventOutbox
    {
        Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode);
        Task<OutboxItem[]> SaveEvents(IEnumerable<Event> @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode);
    }
}
