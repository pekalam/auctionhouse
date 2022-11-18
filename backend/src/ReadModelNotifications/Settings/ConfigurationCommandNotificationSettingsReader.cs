using Microsoft.Extensions.Configuration;

namespace ReadModelNotifications.Settings
{
    internal class ConfigurationCommandNotificationSettingsReader : ICommandNotificationSettingsReader
    {
        private readonly IConfiguration _configuration;

        public ConfigurationCommandNotificationSettingsReader(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public CommandNotificationSettings[] Read()
        {
            var settings = _configuration.GetSection("CommandNotificationSettings").Get<CommandNotificationSettings[]>();
            if (settings is null)
            {
                return Array.Empty<CommandNotificationSettings>();
            }
            ValidateSettings(settings);
            return settings;
        }

        private void ValidateSettings(CommandNotificationSettings[] settings)
        {
            if (settings.Any(s => s is null))
            {
                throw new ArgumentException("Invalid configuration");
            }
            if (settings.Any(s => !s.NotificationsMode.HasValue))
            {
                throw new ArgumentException("Missing NotificationsMode");
            }
            if (settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Saga && s.SagaCompletionCommandNames is null && s.SagaFailureCommandNames is null))
            {
                throw new ArgumentException("Null completion command names");
            }
            if (settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Saga && (s.SagaCompletionCommandNames?.Length == 0 || s.SagaFailureCommandNames?.Length == 0)))
            {
                throw new ArgumentException("Empty completion command names");
            }
            if (settings.Any(s => s.NotificationsMode != ReadModelNotificationsMode.Saga && (s.SagaCompletionCommandNames is not null || s.SagaFailureCommandNames is not null)))
            {
                throw new ArgumentException("Cannot contain other notifications mode than saga and have completion or failure command names");
            }
            if (settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Saga && s.EventsToConfirm is null))
            {
                throw new ArgumentException("Saga notification mode require event confimration to be configured");
            }
            if (settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Immediate && s.EventsToConfirm?.Length > 1))
            {
                throw new ArgumentException("Configuring more than 1 confirmation events is not supported for immediate notifications mode");
            }
        }
    }
}
