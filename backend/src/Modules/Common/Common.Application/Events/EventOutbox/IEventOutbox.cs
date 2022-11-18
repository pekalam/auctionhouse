using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    public interface IEventOutbox
    {
        Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext);
        Task<OutboxItem[]> SaveEvents(IEnumerable<Event> @event, CommandContext commandContext);
    }
}
