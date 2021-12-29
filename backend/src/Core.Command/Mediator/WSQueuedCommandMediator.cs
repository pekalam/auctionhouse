using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;

namespace Core.Command.Mediator
{
    public class WSQueuedCommandMediator : CommandMediator
    {
        private readonly IQueuedCommandBus _queuedCommandBus;
        private readonly IUserIdentityService _userIdentityService;

        public WSQueuedCommandMediator(IUserIdentityService userIdentityService, IImplProvider implProvider, IQueuedCommandBus queuedCommandBus) : base(implProvider)
        {
            _userIdentityService = userIdentityService;
            _queuedCommandBus = queuedCommandBus;
        }

        protected override Task<(RequestStatus, bool)> SendAppCommand<T>(T command)
        {
            var appCommand = new AppCommand<T> { Command = command, CommandContext = CommandContext.CreateWSQueued(_userIdentityService.GetSignedInUserIdentity(), nameof(T)) };
            var requestStatus = new RequestStatus(appCommand.CommandContext.CommandId, Status.PENDING);
            var queuedCommand = new QueuedCommand { AppCommand = appCommand, };

            _queuedCommandBus.Publish<T>(queuedCommand);

            return Task.FromResult((requestStatus, false));
        }
    }
}