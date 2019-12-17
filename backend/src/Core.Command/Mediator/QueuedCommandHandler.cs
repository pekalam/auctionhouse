using System;
using System.Threading;
using Core.Common;
using Core.Common.Command;
using Core.Common.RequestStatusService;
using MediatR;

namespace Core.Command.Mediator
{
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