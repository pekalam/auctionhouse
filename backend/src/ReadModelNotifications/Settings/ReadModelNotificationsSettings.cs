using Common.Application.Commands;
using System.Diagnostics;

namespace ReadModelNotifications.Settings
{

    public class ReadModelNotificationsSettings
    {
        internal const string SagaInitiator_KeyName = "SagaInitiator";

        private readonly ICommandNotificationSettingsReader _settingsReader;

        public ReadModelNotificationsSettings(ICommandNotificationSettingsReader settingsReader)
        {
            _settingsReader = settingsReader;
        }

        private CommandNotificationSettings[] CommandSettings => _settingsReader.Read();


        public ReadModelNotificationsMode GetNotificationMode<T>(AppCommand<T> appCommand) where T : ICommand
        {
            return GetNotificationMode(appCommand.CommandContext.Name);
        }

        public ReadModelNotificationsMode GetNotificationMode(string commandName)
        {
            return CommandSettings.FirstOrDefault(s => s.CommandName == commandName)?.NotificationsMode ?? ReadModelNotificationsMode.Disabled;
        }

        public bool IsSagaCompletionCommand<T>(AppCommand<T> appCommand) where T : ICommand
            => IsSagaCompletionCommand(appCommand.CommandContext);

        public bool IsSagaCompletionCommand(CommandContext commandContext)
        {
            Debug.Assert(commandContext.ExtraData.ContainsKey(SagaInitiator_KeyName));

            return CommandSettings.FirstOrDefault(s => s.CommandName == commandContext.ExtraData[SagaInitiator_KeyName])?.SagaCompletionCommandNames.Contains(commandContext.Name) ?? false;
        }

        public bool IsSagaFailureCommand(CommandContext commandContext)
        {
            Debug.Assert(commandContext.ExtraData.ContainsKey(SagaInitiator_KeyName));

            return CommandSettings.FirstOrDefault(s => s.CommandName == commandContext.ExtraData[SagaInitiator_KeyName])?.SagaFailureCommandNames.Contains(commandContext.Name) ?? false;
        }

        public bool IsEventToConfirmInSaga(CommandContext commandContext, string eventName)
        {
            if (!commandContext.ExtraData.ContainsKey(SagaInitiator_KeyName))
            {
                return false;
            }

            var settings = CommandSettings.FirstOrDefault(s => s.CommandName == commandContext.ExtraData[SagaInitiator_KeyName]);
            if(settings is null)
            {
                return false;
            }

            return settings.NotificationsMode == ReadModelNotificationsMode.Saga && settings.EventsToConfirm.Contains(eventName);
        }

        public bool IsEventToConfirmInImmediateMode(CommandContext commandContext, string eventName)
        {
            var settings = CommandSettings.FirstOrDefault(s => s.CommandName == commandContext.Name);
            if (settings is null)
            {
                return false;
            }

            return settings.NotificationsMode == ReadModelNotificationsMode.Immediate && settings.EventsToConfirm?.Contains(eventName) == true;
        }

        public bool IsInImmediateModeWithoutConfirmationEvent(CommandContext commandContext)
        {
            var settings = CommandSettings.FirstOrDefault(s => s.CommandName == commandContext.Name);

            if(settings is null)
            {
                return false;
            }

            return settings.NotificationsMode == ReadModelNotificationsMode.Immediate && (settings.EventsToConfirm?.Length is null || settings.EventsToConfirm.Length == 0);
        }
    }
}
