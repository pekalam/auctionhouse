using System;
using Common.Application.Events;
using Common.Application.SagaNotifications;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Common.Application.Commands
{
    public abstract class CommandHandlerBase<T> : IRequestHandler<AppCommand<T>, RequestStatus> where T : ICommand
    {
        private readonly ILogger _logger;
        // subclass provides value of this field to decide which mode should be used when saving command status
        private readonly ReadModelNotificationsMode _notificationsMode;
        private readonly Lazy<IImmediateNotifications> _immediateNotifications;
        private readonly Lazy<ISagaNotifications> _sagaNotifications;
        private readonly Lazy<EventBusFacadeWithOutbox> _eventBusWithOutbox;

        protected CommandHandlerBase(ReadModelNotificationsMode notificationsMode, ILogger logger,
            Lazy<IImmediateNotifications> immediateNotifications,
            Lazy<ISagaNotifications> sagaNotifications,
            Lazy<EventBusFacadeWithOutbox> eventBusFacadeWithOutbox)
        {
            _logger = logger;
            _notificationsMode = notificationsMode;
            _immediateNotifications = immediateNotifications;
            _sagaNotifications = sagaNotifications;
            _eventBusWithOutbox = eventBusFacadeWithOutbox;
        }

        private async Task<RequestStatus> TryHandleCommand(AppCommand<T> request, CancellationToken cancellationToken)
        {
            try
            {
                //TODO lazy
                return await HandleCommand(request, new Lazy<EventBusFacade>(() => _eventBusWithOutbox.Value), cancellationToken);
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
                if (_notificationsMode == ReadModelNotificationsMode.Immediate)
                {
                    await _immediateNotifications.Value.RegisterNew(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
                if (_notificationsMode == ReadModelNotificationsMode.Saga)
                {
                    await _sagaNotifications.Value.RegisterNewSaga(request.CommandContext.CorrelationId, request.CommandContext.CommandId);
                }
                var status = await TryHandleCommand(request, cancellationToken);
                _eventBusWithOutbox.Value.ProcessOutbox(); //TODO lazy evenbus in outbox in order to not create it every time

                return status;
            }
            else
            {
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandException($"Invalid command {request}", request.CommandContext);
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request,
            Lazy<EventBusFacade> eventBus, CancellationToken cancellationToken);
    }
}