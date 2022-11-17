using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using ReadModelNotifications.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModelNotifications.EventCallbacks
{
    internal class ReadModelNotificationsEventConsumerCallbacks : IEventConsumerCallbacks
    {
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly ReadModelNotificationsSettings _settigns;

        public ReadModelNotificationsEventConsumerCallbacks(Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications, ReadModelNotificationsSettings settigns)
        {
            _immediateNotifications = immediateNotifications;
            _sagaNotifications = sagaNotifications;
            _settigns = settigns;
        }

        public async Task OnEventProcessed<T>(IAppEvent<T> msg, ILogger logger) where T : Event
        {
            try
            {
                if (_settigns.IsEventToConfirmInSaga(msg.CommandContext, msg.Event.EventName))
                {
                    await _sagaNotifications.Value.MarkEventAsHandled(msg.CommandContext.CorrelationId, msg.Event);
                }
                if (_settigns.IsEventToConfirmInImmediateMode(msg.CommandContext, msg.Event.EventName)
                    || _settigns.IsInImmediateModeWithoutConfirmationEvent(msg.CommandContext))
                {
                    await _immediateNotifications.Value.NotifyCompleted(msg.CommandContext.CorrelationId);
                }
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus("Event consumer notifications processing error");
                logger.LogWarning(e, "Event consumer thrown an exception while processing notifications");
                throw;
            }
        }
    }
}
