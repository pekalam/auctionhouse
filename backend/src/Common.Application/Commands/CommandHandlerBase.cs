using System;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.Common.Domain;
using System.Diagnostics;

namespace Common.Application.Commands
{
    public class CommandHandlerBaseDependencies
    {
        /// <summary>
        /// Test ctor
        /// </summary>
        internal CommandHandlerBaseDependencies()
        {

        }

        public CommandHandlerBaseDependencies(ILogger<RequestStatus> logger, Lazy<IImmediateNotifications> immediateNotifications,
            Lazy<ISagaNotifications> sagaNotifications, IEventOutbox eventOutbox, EventOutboxSender eventOutboxSender, IEventOutboxSavedItems eventOutboxSavedItems)
        {
            Logger = logger;
            ImmediateNotifications = immediateNotifications;
            SagaNotifications = sagaNotifications;
            EventOutbox = eventOutbox;
            EventOutboxSender = eventOutboxSender;
            EventOutboxSavedItems = eventOutboxSavedItems;
        }

        public ILogger<RequestStatus> Logger { get; set; }
        // subclass provides value of this field to decide which mode should be used when saving command status
        public Lazy<IImmediateNotifications> ImmediateNotifications { get; set; }
        public Lazy<ISagaNotifications> SagaNotifications { get; set; }
        public IEventOutbox EventOutbox { get; set; }
        public EventOutboxSender EventOutboxSender { get; set; }
        public IEventOutboxSavedItems EventOutboxSavedItems { get; set; }
    }

    /// <summary>
    /// Adds saga event notifications as part of process of saving event in outbox.
    /// It transparently adds notifications events when command is saving events. //TODO make this process explicit
    /// </summary>
    internal class CommandNotificationsEventOutboxWrapper : IEventOutbox, IEventOutboxSavedItems
    {
        private readonly IEventOutbox _eventOutbox;
        private readonly IEventOutboxSavedItems _eventOutboxSavedItems;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;

        public CommandNotificationsEventOutboxWrapper(IEventOutbox eventOutbox, IEventOutboxSavedItems eventOutboxSavedItems, Lazy<ISagaNotifications> sagaNotifications)
        {
            _eventOutbox = eventOutbox;
            _eventOutboxSavedItems = eventOutboxSavedItems;
            _sagaNotifications = sagaNotifications;
        }

        public IReadOnlyList<OutboxItem> SavedOutboxStoreItems => _eventOutboxSavedItems.SavedOutboxStoreItems;

        public async Task<OutboxItem> SaveEvent(Event @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var item = await _eventOutbox.SaveEvent(@event, commandContext, notificationsMode);
            await AddSagaUnhandledEvents(commandContext);
            return item;
        }

        public async Task<OutboxItem[]> SaveEvents(IEnumerable<Event> @event, CommandContext commandContext, ReadModelNotificationsMode notificationsMode)
        {
            var items = await _eventOutbox.SaveEvents(@event, commandContext, notificationsMode);
            await AddSagaUnhandledEvents(commandContext);
            return items;
        }

        private async Task AddSagaUnhandledEvents(CommandContext commandContext)
        {
            var eventsWithSagaNotificationsMode = _eventOutboxSavedItems.SavedOutboxStoreItems
                .Where(i => i.ReadModelNotifications == ReadModelNotificationsMode.Saga)
                .Select(i => i.Event)
                .ToArray();
            if (eventsWithSagaNotificationsMode.Length > 0)
            {
                await _sagaNotifications.Value.AddUnhandledEvents(commandContext.CorrelationId, eventsWithSagaNotificationsMode);
            }
        }
    }


    public abstract class CommandHandlerBase<T> : IRequestHandler<AppCommand<T>, RequestStatus> where T : ICommand
    {
        private readonly ILogger<RequestStatus> _logger;
        // subclass provides value of this field to decide which mode should be used when saving command status
        private readonly ReadModelNotificationsMode _notificationsMode;
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly IEventOutbox _eventOutbox;
        private readonly EventOutboxSender _eventOutboxSender;
        private readonly IEventOutboxSavedItems _eventOutboxSavedItems;


        protected CommandHandlerBase(ReadModelNotificationsMode notificationsMode, CommandHandlerBaseDependencies dependencies)
        {
            _logger = dependencies.Logger;
            _notificationsMode = notificationsMode;
            _immediateNotifications = dependencies.ImmediateNotifications;
            _sagaNotifications = dependencies.SagaNotifications;
            _eventOutbox = dependencies.EventOutbox;
            _eventOutboxSender = dependencies.EventOutboxSender;
            _eventOutboxSavedItems = dependencies.EventOutboxSavedItems;
        }

        private async Task<RequestStatus> HandleCommandInternal(AppCommand<T> request, CancellationToken cancellationToken)
        {
            try
            {
                var commandNotificationsOutboxWrapper = new CommandNotificationsEventOutboxWrapper(_eventOutbox, _eventOutboxSavedItems, _sagaNotifications);
                return await HandleCommand(request, commandNotificationsOutboxWrapper, cancellationToken);
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus("Command handling error");
                _logger.LogDebug(e, "CommandHandler error {@request}", request);
                throw;
            }
        }

        public virtual async Task<RequestStatus> Handle(AppCommand<T> request, CancellationToken cancellationToken)
        {
            using var activity = Tracing.StartTracing(typeof(T).Name, request.CommandContext.CorrelationId);

            var validationContext = new ValidationContext(request.Command);
            var validationResults = new Collection<ValidationResult>();

            _logger.LogTrace("Handling command {name}", typeof(T).Name);
            if (Validator.TryValidateObject(request.Command, validationContext, validationResults, true))
            {
                await RegisterCommandNotifications(request);

                //when exception is thrown then it should be handled by api
                //failed status can result in events being sent
                var requestStatus = await HandleCommandInternal(request, cancellationToken);               
                await _eventOutboxSender.SendEvents(_eventOutboxSavedItems.SavedOutboxStoreItems);

                if (requestStatus.Status == Status.COMPLETED) Activity.Current.TraceOkStatus();
                if (requestStatus.Status == Status.FAILED) Activity.Current.TraceErrorStatus();
                if (requestStatus.Status == Status.PENDING) Activity.Current.TraceUnsetStatus();
                return requestStatus;
            }
            else
            {
                Activity.Current.TraceErrorStatus("Invalid command validation results");
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandException($"Invalid command {request}", request.CommandContext);
            }
        }

        private async Task RegisterCommandNotifications(AppCommand<T> request) //TODO handle case where already registered or redesign
        {
            try
            {
                if (_notificationsMode == ReadModelNotificationsMode.Immediate)
                {
                    await _immediateNotifications.Value.RegisterNew(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
                if (_notificationsMode == ReadModelNotificationsMode.Saga)
                {
                    await _sagaNotifications.Value.RegisterNewSaga(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus("Notification registration error");
                _logger.LogWarning(e, "Could not register notifications for command {@request}", request);
                throw;
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, IEventOutbox eventOutbox, CancellationToken cancellationToken);
    }
}