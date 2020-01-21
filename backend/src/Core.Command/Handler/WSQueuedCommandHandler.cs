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
        private readonly IRequestStatusService _requestStatusService;
        private readonly IMediator _mediator;
        private readonly ILogger<WSQueuedCommandHandler> _logger;

        public WSQueuedCommandHandler(IRequestStatusService requestStatusService, IMediator mediator,
            ILogger<WSQueuedCommandHandler> logger)
        {
            _requestStatusService = requestStatusService;
            _mediator = mediator;
            _logger = logger;
        }

        public virtual void Handle(CommandBase commandBase)
        {
            RequestStatus requestStatus;
            try
            {
                requestStatus = _mediator.Send(commandBase, CancellationToken.None).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "WSQueued command error {@command}", commandBase);
                _requestStatusService.TrySendRequestFailureToUser(commandBase.GetType().Name,
                    commandBase.CommandContext.CorrelationId, commandBase.CommandContext.User);
                return;
            }

            if (commandBase.CommandContext.User != null)
            {
                if (requestStatus.Status == Status.FAILED)
                {
                    _requestStatusService.TrySendRequestFailureToUser(commandBase.GetType().Name,
                        commandBase.CommandContext.CorrelationId, commandBase.CommandContext.User, requestStatus.ExtraData);
                }
                else if (requestStatus.Status == Status.COMPLETED)
                {
                    _requestStatusService.TrySendRequestCompletionToUser(commandBase.GetType().Name,
                        commandBase.CommandContext.CorrelationId, commandBase.CommandContext.User, requestStatus.ExtraData);
                }
            }
            else
            {
                _logger.LogWarning("Cannot send request status to null user, command: {@command}", commandBase);
            }
        }
    }
}