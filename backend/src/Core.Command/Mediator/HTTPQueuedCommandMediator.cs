using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.EventBus;

[assembly: InternalsVisibleTo("Test.UnitTests")]
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

        public Task<RequestStatus> Send(CommandBase commandBase)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var requestStatus = new RequestStatus(correlationId, Status.PENDING);

            commandBase.CommandContext = new CommandContext()
            {
                CorrelationId = correlationId
            };

            _commandStatusStorage.SaveStatus(requestStatus, commandBase);
            _eventBusService.SendQueuedCommand(commandBase);

            return Task.FromResult(requestStatus);
        }
    }

    public class HTTPQueuedCommandMediator : CommandMediator
    {
        public HTTPQueuedCommandMediator(HTTPQueuedCommandHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }

        public override Task<RequestStatus> Send(CommandBase commandBase)
        {
            commandBase.HttpQueued = true;
            commandBase.WSQueued = false;
            return base.Send(commandBase);
        }
    }
}
