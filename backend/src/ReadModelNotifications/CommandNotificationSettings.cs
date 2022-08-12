using Common.Application.Events;

namespace ReadModelNotifications
{
    // TODO: use alias to setting instead of command name that will be used as a reference for saga handlers that will mark as notified / complete
    /// <summary>
    /// 
    /// </summary>
    public class CommandNotificationSettings
    {
        public string CommandName { get; set; }

        /// <summary>
        /// Nullable to prevent default initialization when configuration is invalid
        /// </summary>
        public ReadModelNotificationsMode? NotificationsMode { get; set; }

        public string[] SagaCompletionCommandNames { get; set; }

        public string[] SagaFailureCommandNames { get; set; }
    }
}
