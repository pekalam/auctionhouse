using Common.Application.Commands;

namespace Common.Application.Mediator
{
    public class WSQueuedCommandMediator : CommandMediator
    {
        private readonly IQueuedCommandBus _queuedCommandBus;
        private readonly IUserIdentityService _userIdentityService;
        private readonly IImplProvider _implProvider;

        public WSQueuedCommandMediator(IUserIdentityService userIdentityService, IImplProvider implProvider, IQueuedCommandBus queuedCommandBus) : base(implProvider)
        {
            _implProvider = implProvider;
            _userIdentityService = userIdentityService;
            _queuedCommandBus = queuedCommandBus;
        }

        public override Task<RequestStatus> Send<T>(T command)
        {
            _queuedCommandBus.PreparePublish(_implProvider, command);
            return base.Send(command);
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