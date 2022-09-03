using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;
using ReadModelNotifications.SagaNotifications;

namespace ReadModelNotifications.ImmediateNotifications
{
    internal class TracedImmediateNotifications : IImmediateNotifications
    {
        private readonly IImmediateNotifications _immediateNotifications;

        public TracedImmediateNotifications(IImmediateNotifications immediateNotifications)
        {
            _immediateNotifications = immediateNotifications;
        }

        public Task NotifyCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var activity = Tracing.StartTracing(nameof(NotifyCompleted), correlationId);
            return _immediateNotifications.NotifyCompleted(correlationId, extraData).AddActivityDisposingContinuation(activity);
        }

        public Task NotifyFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null)
        {
            var activity = Tracing.StartTracing(nameof(NotifyFailed), correlationId);
            return _immediateNotifications.NotifyFailed(correlationId, extraData).AddActivityDisposingContinuation(activity);
        }

        public Task RegisterNew(CorrelationId correlationId, CommandId commandId)
        {
            var activity = Tracing.StartTracing(nameof(RegisterNew), correlationId);
            return _immediateNotifications.RegisterNew(correlationId, commandId).AddActivityDisposingContinuation(activity);
        }
    }
}
