using Core.Common.Command;
using Core.Common.Domain;

namespace Core.Common.EventBus
{
    public enum ReadModelNotificationsMode
    {
        /// <summary>
        /// Query listener can handle event of this type and send notification about completion / failure immediately.
        /// </summary>
        Immediate,
        /// <summary>
        /// Query listener must mark unhandled event of corresponding <see cref="RequestStatus"/> as handled. If <see cref="RequestStatus"/> 
        /// is marked as completed and all events are handled then handler can send notification about completion / failure.
        /// </summary>
        Saga,
        /// <summary>
        /// Query listener should not send any notification.
        /// </summary>
        Disabled,
    }

    public interface IAppEvent<out T> where T : Event
    {
        CommandContext CommandContext { get; }
        T Event { get; }
        ReadModelNotificationsMode ReadModelNotifications { get; }
    }
}