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

        private void TryUpdateCommandStatus(CommandBase commandBase, RequestStatus status)
        {
            try
            {
                _commandStatusStorage.UpdateCommandStatus(status, commandBase);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Cannot save http queued command {@command} status to status storage", commandBase);
            }
        }

        public virtual void Handle(CommandBase commandBase)
        {
            RequestStatus result = null;
            try
            {
                result = _mediator.Send(commandBase).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "HTTPQueued command error {@command}", commandBase);
                var failedStatus = RequestStatus.CreateFromCommandContext(commandBase.CommandContext, Status.FAILED, exception: e);
                TryUpdateCommandStatus(commandBase, failedStatus);
                return;
            }

            if (result.Status != Status.PENDING)
            {
                TryUpdateCommandStatus(commandBase, result);
            }
        }
    }
}
