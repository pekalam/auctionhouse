using Common.Application.Commands;
using Common.Application.Queries;

namespace Common.Application.Mediator
{
    public class WSQueuedCommandMediator : CommandQueryMediator
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

        public override Task<T> Send<T>(IQuery<T> query)
        {
            throw new NotImplementedException();
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