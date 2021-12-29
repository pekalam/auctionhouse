using System;
using System.Collections.Generic;
using System.Text;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public class HTTPQueuedCommandHandler
    {
        private readonly IHTTPQueuedCommandStatusStorage _commandStatusStorage;
        private readonly IMediator _mediator;
        private readonly ILogger<HTTPQueuedCommandHandler> _logger;

        public HTTPQueuedCommandHandler(IHTTPQueuedCommandStatusStorage commandStatusStorage, IMediator mediator, ILogger<HTTPQueuedCommandHandler> logger)
        {
            _commandStatusStorage = commandStatusStorage;
            _mediator = mediator;
            _logger = logger;
        }

        private void TryUpdateCommandStatus(CommandContext commandContext, RequestStatus status)
        {
            try
            {
                _commandStatusStorage.UpdateCommandStatus(status, commandContext.CommandId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot save http queued command {@command} status to status storage", commandContext);
            }
        }

        public virtual void Handle(QueuedCommand command)
        {
            RequestStatus result = null;
            ICommandContextOwner commandContextOwner = (ICommandContextOwner)command.AppCommand;
            try
            {
                result = _mediator.Send((IRequest<RequestStatus>)command.AppCommand).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "HTTPQueued command error {@command}", command);
                var failedStatus = RequestStatus.CreateFromCommandContext(commandContextOwner.CommandContext, Status.FAILED, exception: e);
                TryUpdateCommandStatus(commandContextOwner.CommandContext, failedStatus);
                return;
            }

            if (result.Status != Status.PENDING)
            {
                TryUpdateCommandStatus(commandContextOwner.CommandContext, result);
            }
        }
    }
}
