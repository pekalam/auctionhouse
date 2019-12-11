using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.EventBus;
using Core.Common.RequestStatusService;
using MediatR;

namespace Core.Common.ApplicationServices
{
    public class EventBusService
    {
        private readonly IEventBus _eventBus;
        private readonly IAppEventBuilder _appEventBuilder;

        public EventBusService(IEventBus eventBus, IAppEventBuilder appEventBuilder)
        {
            _eventBus = eventBus;
            _appEventBuilder = appEventBuilder;
        }

        public virtual void Publish<T>(T @event, CorrelationId correlationId, ICommand command) where T : Event
        {
            var appEvent = _appEventBuilder
                .WithCommand(command)
                .WithEvent(@event)
                .WithCorrelationId(correlationId)
                .Build<T>();
            _eventBus.Publish(appEvent);
        }

        public virtual void Publish(IEnumerable<Event> events, CorrelationId correlationId, ICommand command)
        {
            foreach (var @event in events)
            {
                Publish(@event, correlationId, command);
            }
        }

        public virtual void SendQueuedCommand(ICommand command)
        {
            _eventBus.Send(command);
        }
    }

    public class QueuedCommandHandler
    {
        private IRequestStatusService _requestStatusService;
        private IMediator _mediator;

        public QueuedCommandHandler(IRequestStatusService requestStatusService, IMediator mediator)
        {
            _requestStatusService = requestStatusService;
            _mediator = mediator;
        }

        public virtual void Handle(ICommand command, Type handlerType)
        {
            RequestStatus requestStatus;
            try
            {
                requestStatus = _mediator.Send(command, CancellationToken.None).Result;

            }
            catch (Exception)
            {
                _requestStatusService.TrySendRequestFailureToUser(command.GetType().Name, command.CommandContext.CorrelationId, command.CommandContext.User);
                return;
            }

            if (requestStatus.Status == Status.FAILED)
            {
                if (command.CommandContext.User != null)
                {
                    _requestStatusService.TrySendRequestFailureToUser(command.GetType().Name, command.CommandContext.CorrelationId, command.CommandContext.User);
                }
            }
            else if (requestStatus.Status == Status.COMPLETED)
            {
                if (command.CommandContext.User != null)
                {
                    _requestStatusService.TrySendRequestCompletionToUser(command.GetType().Name, command.CommandContext.CorrelationId, command.CommandContext.User);
                }
            }
        }
    }
}
