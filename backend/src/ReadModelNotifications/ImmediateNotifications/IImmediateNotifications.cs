using Common.Application.Commands;
using Common.Application.Events;

namespace ReadModelNotifications.ImmediateNotifications
{
    /// <summary>
    /// Responsible for sending notification / saving outcome of command.
    /// Listeners can use this interface to notify when notifications mode=<see cref="ReadModelNotificationsMode.Immediate"/>
    /// </summary>
    public interface IImmediateNotifications
    {
        Task RegisterNew(CorrelationId correlationId, CommandId commandId);
        Task NotifyCompleted(CorrelationId correlationId, Dictionary<string, object>? extraData = null);
        Task NotifyFailed(CorrelationId correlationId, Dictionary<string, object>? extraData = null);
    }
}