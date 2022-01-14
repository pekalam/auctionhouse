using System;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.Common.Domain;

namespace Common.Application.Commands
{
    public class CommandHandlerBaseDependencies
    {
        public CommandHandlerBaseDependencies(ILogger<RequestStatus> logger, Lazy<IImmediateNotifications> immediateNotifications,
            Lazy<ISagaNotifications> sagaNotifications, IEventOutbox eventOutbox, EventOutboxSender eventOutboxSender)
        {
            Logger = logger;
            ImmediateNotifications = immediateNotifications;
            SagaNotifications = sagaNotifications;
            EventOutbox = eventOutbox;
            EventOutboxSender = eventOutboxSender;
        }

        public ILogger<RequestStatus> Logger { get; set; }
        // subclass provides value of this field to decide which mode should be used when saving command status
        public Lazy<IImmediateNotifications> ImmediateNotifications { get; set; }
        public Lazy<ISagaNotifications> SagaNotifications { get; set; }
        public IEventOutbox EventOutbox { get; set; }
        public EventOutboxSender EventOutboxSender { get; set; }
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


        protected CommandHandlerBase(ReadModelNotificationsMode notificationsMode, CommandHandlerBaseDependencies dependencies)
        {
            _logger = dependencies.Logger;
            _notificationsMode = notificationsMode;
            _immediateNotifications = dependencies.ImmediateNotifications;
            _sagaNotifications = dependencies.SagaNotifications;
            _eventOutbox = dependencies.EventOutbox;
            _eventOutboxSender = dependencies.EventOutboxSender;
        }

        private async Task<RequestStatus> TryHandleCommand(AppCommand<T> request, CancellationToken cancellationToken)
        {
            try
            {
                //TODO lazy
                return await HandleCommand(request, _eventOutbox, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "CommandHandler error {@request}", request);
                throw;
            }
        }

        public virtual async Task<RequestStatus> Handle(AppCommand<T> request, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new Collection<ValidationResult>();

            _logger.LogTrace("Handling command {name}", typeof(T).Name);
            if (Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                await RegisterCommandNotifications(request);

                var status = await TryHandleCommand(request, cancellationToken);

                //TODO handle exception and remove notifications

                var eventOutbox = (EventOutbox)_eventOutbox; //TODO remove casting ?
                await AddSagaUnhandledEvents(request, eventOutbox);
                await _eventOutboxSender.SendEvents(eventOutbox.SavedOutboxStoreItems);

                return status;
            }
            else
            {
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandException($"Invalid command {request}", request.CommandContext);
            }
        }

        private async Task AddSagaUnhandledEvents(AppCommand<T> request, EventOutbox eventOutbox)
        {
            var eventsWithSagaNotificationsMode = eventOutbox.SavedOutboxStoreItems
                .Where(i => i.ReadModelNotifications == ReadModelNotificationsMode.Saga)
                .Select(i => i.Event)
                .ToArray();
            if (eventsWithSagaNotificationsMode.Length > 0)
            {
                await _sagaNotifications.Value.AddUnhandledEvents(request.CommandContext.CorrelationId, eventsWithSagaNotificationsMode);
            }
        }

        private async Task RegisterCommandNotifications(AppCommand<T> request)
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

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, IEventOutbox eventOutbox, CancellationToken cancellationToken);
    }
}