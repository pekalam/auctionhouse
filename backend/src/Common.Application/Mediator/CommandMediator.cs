using Common.Application.Commands;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Common.Application.Mediator
{
    public abstract class CommandMediator
    {
        private readonly IImplProvider _implProvider;

        protected CommandMediator(IImplProvider implProvider)
        {
            _implProvider = implProvider;
        }

        internal static void InvokePostCommandAttributeStrategies(IImplProvider implProvider, CommandContext commandContext, ICommand command)
        {
            if (AttributeStrategies.PostHandleCommandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in AttributeStrategies.PostHandleCommandAttributeStrategies[command.GetType()])
                {
                    strategy?.Invoke(implProvider, commandContext, command);
                }
            }
        }

        public virtual async Task<RequestStatus> Send<T>(T command) where T : ICommand
        {
            var appCommand = new AppCommand<T>
            {
                Command = command,
                CommandContext = CommandContext.CreateNew(typeof(T).Name),
            };

            if (AttributeStrategies.PreHandleCommandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in AttributeStrategies.PreHandleCommandAttributeStrategies[command.GetType()])
                {
                    strategy?.Invoke(_implProvider, appCommand.CommandContext, command);
                }
            }

            var (requestStatus, callPostActions) = await SendAppCommand(appCommand);

            if (callPostActions)
            {
                InvokePostCommandAttributeStrategies(_implProvider, appCommand.CommandContext, command);
            }

            return requestStatus;
        }

        protected abstract Task<(RequestStatus, bool)> SendAppCommand<T>(AppCommand<T> command) where T : ICommand;
    }
}