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
            catch (Exception)
            {
                _logger.LogError(
                    $"Cannot save http queued command {command.CommandContext.CorrelationId.Value} status to status storage");
            }
        }

        public virtual void Handle(ICommand command)
        {
            RequestStatus status = null;
            try
            {
                status = _mediator.Send(command).Result;
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Command with correlationId: {command.CommandContext.CorrelationId.Value} has failed {e.ToString()}");
                var failedStatus = RequestStatus.CreateFromCommandContext(command.CommandContext, Status.FAILED, exception: e);
                TryUpdateCommandStatus(command, failedStatus);
                return;
            }

            TryUpdateCommandStatus(command, status);
        }
    }
}
