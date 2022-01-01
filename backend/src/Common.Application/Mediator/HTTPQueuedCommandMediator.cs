using Common.Application.Commands;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Common.Application.Mediator
{
    public interface IQueuedCommandBus
    {
        /// <summary>
        /// Publishes queued command.
        /// </summary>
        /// <typeparam name="TCmd">Type of <see cref="QueuedCommand.AppCommand"/>. Bus implementation can use this param to create unique queue per command type.</typeparam>
        /// <param name="command"></param>
        void Publish<TCmd>(QueuedCommand command);
        void PreparePublish(IImplProvider implProvider, ICommand command);
        event Action<EventArgs, ILogger> Disconnected;
    }

    public class QueuedCommand
    {
        public object AppCommand { get; set; }
    }

    public class HTTPQueuedCommandMediator : CommandMediator
    {
        private readonly IQueuedCommandBus _queuedCommandBus;
        private readonly IUserIdentityService _userIdentityService;
        private readonly IImplProvider _implProvider;

        public HTTPQueuedCommandMediator(IImplProvider implProvider, IQueuedCommandBus queuedCommandBus) : base(implProvider)
        {
            _queuedCommandBus = queuedCommandBus;
            _implProvider = implProvider;
        }

        public override Task<RequestStatus> Send<T>(T command)
        {
            _queuedCommandBus.PreparePublish(_implProvider, command);
            return base.Send(command);
        }

        protected override async Task<(RequestStatus, bool)> SendAppCommand<T>(T command)
        {
            var appCommand = new AppCommand<T> { Command = command, CommandContext = CommandContext.CreateHttpQueued(_userIdentityService.GetSignedInUserIdentity(), nameof(T)) };
            var requestStatus = new RequestStatus(appCommand.CommandContext.CommandId, Status.PENDING);

            var queuedCommand = new QueuedCommand { AppCommand = appCommand, };

            _queuedCommandBus.Publish<T>(queuedCommand);

            return (requestStatus, false);
        }

    }
}
