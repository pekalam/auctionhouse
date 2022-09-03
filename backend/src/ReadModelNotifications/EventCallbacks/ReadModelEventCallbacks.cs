using Common.Application;
using Common.Application.Events;
using Core.Common.Domain;
using Core.Query.EventHandlers;
using Microsoft.Extensions.Logging;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModelNotifications.EventCallbacks
{
    internal class ReadModelEventCallbacks : IEventConsumerCallbacks
    {
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;

        public ReadModelEventCallbacks(Lazy<IImmediateNotifications> immediateNotifications, Lazy<ISagaNotifications> sagaNotifications)
        {
            _immediateNotifications = immediateNotifications;
            _sagaNotifications = sagaNotifications;
        }

        public async Task OnEventProcessed<T>(IAppEvent<T> msg, ILogger logger) where T : Event
        {
            try
            {
                if (msg.ReadModelNotifications == ReadModelNotificationsMode.Saga)
                {
                    await _sagaNotifications.Value.MarkEventAsHandled(msg.CommandContext.CorrelationId, msg.Event);
                }
                if (msg.ReadModelNotifications == ReadModelNotificationsMode.Immediate)
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
