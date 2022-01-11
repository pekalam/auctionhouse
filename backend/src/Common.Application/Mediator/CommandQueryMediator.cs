using Common.Application.Commands;
using Common.Application.Queries;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Common.Application.Mediator
{
    public abstract class CommandQueryMediator
    {
        private readonly IImplProvider _implProvider;

        protected CommandQueryMediator(IImplProvider implProvider)
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

        public virtual async Task<RequestStatus> Send<T>(T command, CommandContext? commandContext = null) where T : ICommand
        {
            var appCommand = new AppCommand<T>
            {
                Command = command,
                CommandContext = commandContext ?? CommandContext.CreateNew(typeof(T).Name),
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

        public async Task<T> SendQuery<T>(IQuery<T> query)
        {
            if (AttributeStrategies.PreHandleQueryAttributeStrategies.ContainsKey(query.GetType()))
            {
                foreach (var strategy in AttributeStrategies.PreHandleQueryAttributeStrategies[query.GetType()])
                {
                    strategy?.Invoke(_implProvider, query);
                }
            }
            var result = await Send(query);

            if (AttributeStrategies.PostHandleQueryAttributeStrategies.ContainsKey(query.GetType()))
            {
                foreach (var strategy in AttributeStrategies.PostHandleQueryAttributeStrategies[query.GetType()])
                {
                    strategy?.Invoke(_implProvider, query);
                }
            }

            return result;
        }

        protected abstract Task<T> Send<T>(IQuery<T> query);

        protected abstract Task<(RequestStatus, bool)> SendAppCommand<T>(AppCommand<T> command) where T : ICommand;
    }
}