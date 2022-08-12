using Common.Application.Commands;
using Common.Application.Events;
using System.Diagnostics;

namespace ReadModelNotifications
{

    public class ReadModelNotificationsSettings
    {
        internal const string SagaInitiator_KeyName = "SagaInitiator";

        private readonly ICommandNotificationSettingsReader _settingsReader;

        public ReadModelNotificationsSettings(ICommandNotificationSettingsReader settingsReader)
        {
            _settingsReader = settingsReader;
        }

        public CommandNotificationSettings[] CommandSettings => _settingsReader.Read();


        public ReadModelNotificationsMode GetNotificationMode<T>(AppCommand<T> appCommand) where T: ICommand
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
    }
}
