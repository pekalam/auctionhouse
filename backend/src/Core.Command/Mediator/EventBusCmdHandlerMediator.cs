using System;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Command.Mediator
{
    public class EventBusCmdHandlerMediator : ICommandHandlerMediator
    {
        private EventBusService _eventBusService;
        private IUserIdentityService _userIdentityService;


        public EventBusCmdHandlerMediator(EventBusService eventBusService, IUserIdentityService userIdentityService)
        {
            _eventBusService = eventBusService;
            _userIdentityService = userIdentityService;
        }


        public Task<RequestStatus> Send(ICommand command)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var signedInUser = _userIdentityService.GetSignedInUserIdentity();
            command.CommandContext = new CommandContext(correlationId, signedInUser);

            _eventBusService.SendQueuedCommand(command);

            var requestStatus = new RequestStatus(correlationId, Status.PENDING);
            return Task.FromResult(requestStatus);
        }
    }
}