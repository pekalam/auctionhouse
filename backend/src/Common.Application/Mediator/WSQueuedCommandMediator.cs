using Common.Application.Commands;

namespace Common.Application.Mediator
{
    public class WSQueuedCommandMediator : CommandMediator
    {
        private readonly IQueuedCommandBus _queuedCommandBus;
        private readonly IImplProvider _implProvider;

        public WSQueuedCommandMediator(IImplProvider implProvider, IQueuedCommandBus queuedCommandBus) : base(implProvider)
        {
            _implProvider = implProvider;
            _queuedCommandBus = queuedCommandBus;
        }

        public override Task<RequestStatus> Send<T>(T command)
        {
            _queuedCommandBus.PreparePublish(_implProvider, command);
            return base.Send(command);
        }

        protected override Task<(RequestStatus, bool)> SendAppCommand<T>(AppCommand<T> appCommand)
        {
            appCommand.CommandContext.WSQueued = true;
            var requestStatus = new RequestStatus(appCommand.CommandContext.CommandId, Status.PENDING);
            var queuedCommand = new QueuedCommand { AppCommand = appCommand, };

            _queuedCommandBus.Publish<T>(queuedCommand);

            return Task.FromResult((requestStatus, false));
        }
    }
}