using Common.Application;
using Common.Application.Commands;
using Common.Application.Commands.Callbacks;
using Common.Application.Events;
using Common.Extensions;
using ReadModelNotifications.ImmediateNotifications;
using ReadModelNotifications.SagaNotifications;
using ReadModelNotifications.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModelNotifications.CommandCallbacks
{


    internal class ReadModelNotificationsCommandCallbacks : ICommandHandlerCallbacks
    {
        private readonly IImmediateNotifications _immediateNotifications;
        private readonly ISagaNotifications _sagaNotifications;
        private readonly ReadModelNotificationsSettings _settings;

        private ReadModelNotificationsMode? _overridenMode;

        public ReadModelNotificationsCommandCallbacks(ReadModelNotificationsSettings settings, ISagaNotifications sagaNotifications, IImmediateNotifications immediateNotifications)
        {
            _settings = settings;
            _sagaNotifications = sagaNotifications;
            _immediateNotifications = immediateNotifications;
        }

        public async Task OnExecute<T>(AppCommand<T> appCommand) where T : ICommand
        {
            var notificationMode = _overridenMode ?? _settings.GetNotificationMode(appCommand);
            if (notificationMode == ReadModelNotificationsMode.Disabled)
            {
                return;
            }

            // when is part of a saga
            if (appCommand.CommandContext.ExtraData.HasSagaInitiatorKey())
            {
                return;
            }

            await RegisterCommandNotifications(appCommand, notificationMode);

            appCommand.CommandContext.ExtraData[ReadModelNotificationsSettings.SagaInitiator_KeyName] = appCommand.CommandContext.Name;
        }

        private async Task RegisterCommandNotifications<T>(AppCommand<T> request, ReadModelNotificationsMode notificationMode) where T : ICommand //TODO handle case where already registered or redesign
        {

            try
            {
                if (notificationMode == ReadModelNotificationsMode.Immediate)
                {
                    await _immediateNotifications.RegisterNew(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
                if (notificationMode == ReadModelNotificationsMode.Saga)
                {
                    await _sagaNotifications.RegisterNewSaga(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus("Notification registration error");
                //_logger.LogWarning(e, "Could not register notifications for command {@request}", request);
                throw;
            }
        }


        public Task OnCompleted<T>(AppCommand<T> appCommand) where T : ICommand
        {
            return Task.CompletedTask;
        }

        public Task OnEventsSent(IReadOnlyList<OutboxItem> items)
        {
            return Task.CompletedTask;
        }

        public Task CallExtension(string key, string value)
        {
            if (key == CommonExtensionKeys.ReadModelNotificationsMode)
            {
                _overridenMode = value switch
                {
                    CommonExtensionKeys.ReadModelNotificationsDisabledMode => ReadModelNotificationsMode.Disabled,
                    CommonExtensionKeys.ReadModelNotificationsSagaMode => ReadModelNotificationsMode.Saga,
                    CommonExtensionKeys.ReadModelNotificationsImmediateMode => ReadModelNotificationsMode.Immediate,
                };
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Command solely used for completing saga if current command is configured to be
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="appCommand"></param>
        /// <returns></returns>
        public async Task OnUowCommit<T>(AppCommand<T> appCommand) where T : ICommand
        {
            await MarkSagaAsCompletedByCommandContext(appCommand.CommandContext);
        }

        public async Task OnUowCommit(CommandContext commandContext)
        {
            await MarkSagaAsCompletedByCommandContext(commandContext);
        }

        private Task MarkSagaAsCompletedByCommandContext(CommandContext commandContext)
        {
            if (!commandContext.ExtraData.HasSagaInitiatorKey())
            {
                return Task.CompletedTask;
            }

            if (_settings.GetNotificationMode(commandContext.ExtraData[ReadModelNotificationsSettings.SagaInitiator_KeyName]) == ReadModelNotificationsMode.Saga)
            {
                if (_settings.IsSagaCompletionCommand(commandContext))
                {
                    return _sagaNotifications.MarkSagaAsCompleted(commandContext.CorrelationId);
                }
                if (_settings.IsSagaFailureCommand(commandContext))
                {
                    return _sagaNotifications.MarkSagaAsFailed(commandContext.CorrelationId);
                }
            }

            return Task.CompletedTask;
        }
    }
}
