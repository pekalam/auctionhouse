using Common.Application.Events;
using Microsoft.Extensions.Configuration;

namespace ReadModelNotifications
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
            if(settings is null)
            {
                return Array.Empty<CommandNotificationSettings>();
            }
            ValidateSettings(settings);
            return settings;
        }

        private void ValidateSettings(CommandNotificationSettings[] settings)
        {
            if(settings.Any(s => s is null))
            {
                throw new ArgumentException("Invalid configuration");
            }
            if(settings.Any(s => !s.NotificationsMode.HasValue))
            {
                throw new ArgumentException("Missing NotificationsMode");
            }
            if(settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Saga && (s.SagaCompletionCommandNames is null && s.SagaFailureCommandNames is null)))
            {
                throw new ArgumentException("Null completion command names");
            }
            if (settings.Any(s => s.NotificationsMode == ReadModelNotificationsMode.Saga && (s.SagaCompletionCommandNames?.Length == 0 || s.SagaFailureCommandNames?.Length == 0)))
            {
                throw new ArgumentException("Empty completion command names");
            }
            if(settings.Any(s => s.NotificationsMode != ReadModelNotificationsMode.Saga && (s.SagaCompletionCommandNames is not null || s.SagaFailureCommandNames is not null)) )
            {
                throw new ArgumentException("Cannot contain other notifications mode than saga and have completion or failure command names");
            }
        }
    }
}
