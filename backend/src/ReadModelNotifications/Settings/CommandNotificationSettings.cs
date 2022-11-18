namespace ReadModelNotifications.Settings
{
    // TODO: use alias to setting instead of command name that will be used as a reference for saga handlers that will mark as notified / complete
    /// <summary>
    /// 
    /// </summary>
    public class CommandNotificationSettings
    {
        public string CommandName { get; set; }


        /// Nullable to prevent default initialization when configuration is invalid
        public ReadModelNotificationsMode? NotificationsMode { get; set; }

        public string[] SagaCompletionCommandNames { get; set; }

        public string[] SagaFailureCommandNames { get; set; }

        /// <summary>
        /// Events to confirm for Saga or Immediate mode
        /// </summary>
        public string[] EventsToConfirm { get; set; }
    }
}
