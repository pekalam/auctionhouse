using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.EventBus;

[assembly: InternalsVisibleTo("UnitTests")]
namespace Core.Command.Mediator
{
    public class HTTPQueuedCommandHandlerMediator : ICommandHandlerMediator
    {
        private IHTTPQueuedCommandStatusStorage _commandStatusStorage;
        private EventBusService _eventBusService;

        public HTTPQueuedCommandHandlerMediator(IHTTPQueuedCommandStatusStorage commandStatusStorage, EventBusService eventBusService)
        {
            _commandStatusStorage = commandStatusStorage;
            _eventBusService = eventBusService;
        }

        public Task<RequestStatus> Send(ICommand command)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var requestStatus = new RequestStatus(correlationId, Status.PENDING);

            command.CommandContext = new CommandContext()
            {
                CorrelationId = correlationId
            };

            _commandStatusStorage.SaveStatus(requestStatus, command);
            _eventBusService.SendQueuedCommand(command);

            return Task.FromResult(requestStatus);
        }
    }

    public class HTTPQueuedCommandMediator : CommandMediator
    {
        public HTTPQueuedCommandMediator(HTTPQueuedCommandHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }

        public override Task<RequestStatus> Send(ICommand command)
        {
            command.HttpQueued = true;
            command.WSQueued = false;
            return base.Send(command);
        }
    }
}
