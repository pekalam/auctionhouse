using System;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.EventBus;

namespace Core.Command.Mediator
{
    public class EventBusCommandHandlerMediator : ICommandHandlerMediator
    {
        private EventBusService _eventBusService;
        private IUserIdentityService _userIdentityService;

        public EventBusCommandHandlerMediator(EventBusService eventBusService, IUserIdentityService userIdentityService)
        {
            _eventBusService = eventBusService;
            _userIdentityService = userIdentityService;
        }

        public Task<RequestStatus> Send(ICommand command)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var requestStatus = new RequestStatus(correlationId, Status.PENDING);

            var signedInUser = _userIdentityService.GetSignedInUserIdentity();
            command.CommandContext = new CommandContext() { CorrelationId = correlationId, User = signedInUser };

            _eventBusService.SendQueuedCommand(command);

            return Task.FromResult(requestStatus);
        }
    }
}