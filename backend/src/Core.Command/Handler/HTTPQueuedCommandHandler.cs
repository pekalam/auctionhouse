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
        private IHTTPQueuedCommandStatusStorage _commandStatusStorage;
        private readonly IMediator _mediator;
        private readonly ILogger<HTTPQueuedCommandHandler> _logger;

        public HTTPQueuedCommandHandler(IHTTPQueuedCommandStatusStorage commandStatusStorage, IMediator mediator, ILogger<HTTPQueuedCommandHandler> logger)
        {
            _commandStatusStorage = commandStatusStorage;
            _mediator = mediator;
            _logger = logger;
        }

        private void TryUpdateCommandStatus(ICommand command, RequestStatus status)
        {
            try
            {
                _commandStatusStorage.UpdateCommandStatus(status, command);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot save http queued command {@command} status to status storage", command);
            }
        }

        public virtual void Handle(ICommand command)
        {
            RequestStatus result = null;
            try
            {
                result = _mediator.Send(command).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Command error {@command}", command);
                var failedStatus = RequestStatus.CreateFromCommandContext(command.CommandContext, Status.FAILED, exception: e);
                TryUpdateCommandStatus(command, failedStatus);
                return;
            }

            if (result.Status != Status.PENDING)
            {
                TryUpdateCommandStatus(command, result);
            }
        }
    }
}
