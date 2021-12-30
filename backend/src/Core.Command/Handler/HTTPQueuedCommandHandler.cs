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
        private readonly IMediator _mediator;
        private readonly ILogger<HTTPQueuedCommandHandler> _logger;

        public HTTPQueuedCommandHandler(IMediator mediator, ILogger<HTTPQueuedCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public virtual void Handle(QueuedCommand command)
        {
            //TODO
            ICommandContextOwner commandContextOwner = (ICommandContextOwner)command.AppCommand;
            _logger.LogDebug("Handling queued command {id}", commandContextOwner.CommandContext.CommandId.Id);
            _ = _mediator.Send((IRequest<RequestStatus>)command.AppCommand).Result;
        }
    }
}
