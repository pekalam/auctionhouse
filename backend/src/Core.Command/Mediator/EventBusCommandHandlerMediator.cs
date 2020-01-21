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
        private readonly EventBusService _eventBusService;
        private readonly IUserIdentityService _userIdentityService;

        public EventBusCommandHandlerMediator(EventBusService eventBusService, IUserIdentityService userIdentityService)
        {
            _eventBusService = eventBusService;
            _userIdentityService = userIdentityService;
        }

        public Task<RequestStatus> Send(CommandBase commandBase)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var requestStatus = new RequestStatus(correlationId, Status.PENDING);

            var signedInUser = _userIdentityService.GetSignedInUserIdentity();
            commandBase.CommandContext = new CommandContext() { CorrelationId = correlationId, User = signedInUser };

            _eventBusService.SendQueuedCommand(commandBase);

            return Task.FromResult(requestStatus);
        }
    }
}