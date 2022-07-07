using Common.Application;
using Common.Application.Commands;
using Common.Application.Mediator;
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
