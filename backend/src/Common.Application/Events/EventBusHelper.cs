using Common.Application.Commands;
using Core.Common.Domain;

namespace Common.Application.Events
{
    //public class EventBusHelper
    //{
    //    private readonly IEventBus _eventBus;
    //    private readonly IAppEventBuilder _appEventBuilder;

    //    public EventBusHelper(IEventBus eventBus, IAppEventBuilder appEventBuilder)
    //    {
    //        _eventBus = eventBus;
    //        _appEventBuilder = appEventBuilder;
    //    }

    //    public void Publish(IEnumerable<Event> events, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
    //    {
    //        foreach (var @event in events)
    //        {
    //            Publish(@event, commandContext, notificationsMode);
    //        }
    //    }

    //    public void Publish(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
    //    {
    //        var appEvent = _appEventBuilder.WithCommandContext(commandContext)
    //                .WithEvent(@event)
    //                .WithReadModelNotificationsMode(notificationsMode)
    //                .Build<Event>();

    //        _eventBus.Publish(appEvent);
    //    }
    //}
}
