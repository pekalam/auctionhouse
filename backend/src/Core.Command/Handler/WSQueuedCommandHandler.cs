using System;
using System.Threading;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.RequestStatusSender;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public class WSQueuedCommandHandler
    {
        private readonly IRequestStatusSender _requestStatusService;
        private readonly IMediator _mediator;
        private readonly ILogger<WSQueuedCommandHandler> _logger;

        public WSQueuedCommandHandler(IRequestStatusSender requestStatusService, IMediator mediator,
            ILogger<WSQueuedCommandHandler> logger)
        {
            _requestStatusService = requestStatusService;
            _mediator = mediator;
            _logger = logger;
        }

        public virtual void Handle(QueuedCommand command)
        {
            RequestStatus requestStatus;
            ICommandContextOwner commandContextOwner = (ICommandContextOwner)command.AppCommand;
            try
            {
                requestStatus = _mediator.Send((IRequest<RequestStatus>)command.AppCommand, CancellationToken.None).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "WSQueued command error {@command}", command);
                _requestStatusService.TrySendRequestFailureToUser(commandContextOwner.CommandContext.Name,
                    commandContextOwner.CommandContext.CommandId, commandContextOwner.CommandContext.User);
                return;
            }

            if (commandContextOwner.CommandContext.User != Guid.Empty)
            {
                if (requestStatus.Status == Status.FAILED)
                {
                    _requestStatusService.TrySendRequestFailureToUser(commandContextOwner.CommandContext.Name,
                        commandContextOwner.CommandContext.CommandId, commandContextOwner.CommandContext.User, requestStatus.ExtraData);
                }
                else if (requestStatus.Status == Status.COMPLETED)
                {
                    _requestStatusService.TrySendRequestCompletionToUser(commandContextOwner.CommandContext.Name,
                        commandContextOwner.CommandContext.CommandId, commandContextOwner.CommandContext.User, requestStatus.ExtraData);
                }
            }
            else
            {
                _logger.LogWarning("Cannot send request status to null user, command: {@command}", command);
            }
        }
    }
}