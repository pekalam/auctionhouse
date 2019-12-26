using System;
using System.Threading;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.RequestStatusService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public class WSQueuedCommandHandler
    {
        private IRequestStatusService _requestStatusService;
        private IMediator _mediator;
        private ILogger<WSQueuedCommandHandler> _logger;

        public WSQueuedCommandHandler(IRequestStatusService requestStatusService, IMediator mediator,
            ILogger<WSQueuedCommandHandler> logger)
        {
            _requestStatusService = requestStatusService;
            _mediator = mediator;
            _logger = logger;
        }

        public virtual void Handle(ICommand command)
        {
            RequestStatus requestStatus;
            try
            {
                requestStatus = _mediator.Send(command, CancellationToken.None).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Command error {@command}", command);
                _requestStatusService.TrySendRequestFailureToUser(command.GetType().Name,
                    command.CommandContext.CorrelationId, command.CommandContext.User);
                return;
            }

            if (command.CommandContext.User != null)
            {
                if (requestStatus.Status == Status.FAILED)
                {
                    _requestStatusService.TrySendRequestFailureToUser(command.GetType().Name,
                        command.CommandContext.CorrelationId, command.CommandContext.User);
                }
                else if (requestStatus.Status == Status.COMPLETED)
                {
                    _requestStatusService.TrySendRequestCompletionToUser(command.GetType().Name,
                        command.CommandContext.CorrelationId, command.CommandContext.User);
                }
            }
            else
            {
                _logger.LogWarning("Cannot send request status to null user, command: {@command}", command);
            }
        }
    }
}