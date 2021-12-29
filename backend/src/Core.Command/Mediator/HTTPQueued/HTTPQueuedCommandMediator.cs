using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Command.Mediator
{
    public interface IQueuedCommandBus
    {
        /// <summary>
        /// Publishes queued command.
        /// </summary>
        /// <typeparam name="TCmd">Type of <see cref="QueuedCommand.AppCommand"/>. Bus implementation can use this param to create unique queue per command type.</typeparam>
        /// <param name="command"></param>
        void Publish<TCmd>(QueuedCommand command);
        event Action<EventArgs, ILogger> Disconnected;
    }

    public class QueuedCommand
    {
        public object AppCommand { get; set; }
    }

    public class HTTPQueuedCommandMediator : CommandMediator
    {
        private readonly IHTTPQueuedCommandStatusStorage _commandStatusStorage;
        private readonly IQueuedCommandBus _queuedCommandBus;
        private readonly IUserIdentityService _userIdentityService;

        public HTTPQueuedCommandMediator(IImplProvider implProvider, IQueuedCommandBus queuedCommandBus) : base(implProvider)
        {
            _queuedCommandBus = queuedCommandBus;
        }

        protected override Task<(RequestStatus, bool)> SendAppCommand<T>(T command)
        {
            var appCommand = new AppCommand<T> { Command = command, CommandContext = CommandContext.CreateHttpQueued(_userIdentityService.GetSignedInUserIdentity(), nameof(T)) };
            var requestStatus = new RequestStatus(appCommand.CommandContext.CommandId, Status.PENDING);

            var queuedCommand = new QueuedCommand { AppCommand = appCommand, };

            _commandStatusStorage.SaveStatus(requestStatus, appCommand.Command);
            _queuedCommandBus.Publish<T>(queuedCommand);

            return Task.FromResult((requestStatus, false));
        }

    }
}
