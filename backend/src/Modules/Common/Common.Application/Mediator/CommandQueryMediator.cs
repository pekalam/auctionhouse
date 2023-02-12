using Common.Application.Commands;
using Common.Application.Queries;
using MediatR;

namespace Common.Application.Mediator
{
    public class CommandQueryMediator
    {
        private readonly IImplProvider _implProvider;
        private readonly IMediator _mediator;

        public CommandQueryMediator(IImplProvider implProvider, IMediator mediator)
        {
            _implProvider = implProvider;
            _mediator = mediator;
        }

        public virtual async Task<RequestStatus> Send<T>(T command, CommandContext? commandContext = null) where T : ICommand
        {
            var commandType = command.GetType();
            var appCommand = new AppCommand<T>
            {
                Command = command,
                CommandContext = commandContext?.CloneWithName(commandType.Name) ?? CommandContext.CreateNew(commandType.Name),
            };

            if (AttributeStrategies.PreHandleCommandAttributeStrategies.ContainsKey(commandType))
            {
                foreach (var strategy in AttributeStrategies.PreHandleCommandAttributeStrategies[commandType])
                {
                    strategy?.Invoke(_implProvider, appCommand.CommandContext, command);
                }
            }

            var requestStatus = await _mediator.Send(appCommand);

            InvokePostCommandAttributeStrategies(_implProvider, appCommand.CommandContext, command, commandType);

            return requestStatus;
        }

        public virtual async Task<T> SendQuery<T>(IQuery<T> query)
        {
            var queryType = query.GetType();
            if (AttributeStrategies.PreHandleQueryAttributeStrategies.ContainsKey(queryType))
            {
                foreach (var strategy in AttributeStrategies.PreHandleQueryAttributeStrategies[queryType])
                {
                    strategy?.Invoke(_implProvider, query);
                }
            }
            var result = await _mediator.Send(query);

            if (AttributeStrategies.PostHandleQueryAttributeStrategies.ContainsKey(queryType))
            {
                foreach (var strategy in AttributeStrategies.PostHandleQueryAttributeStrategies[queryType])
                {
                    strategy?.Invoke(_implProvider, query);
                }
            }

            return result;
        }


        internal static void InvokePostCommandAttributeStrategies(IImplProvider implProvider, CommandContext commandContext, ICommand command, Type commandType)
        {
            if (AttributeStrategies.PostHandleCommandAttributeStrategies.ContainsKey(commandType))
            {
                foreach (var strategy in AttributeStrategies.PostHandleCommandAttributeStrategies[commandType])
                {
                    strategy?.Invoke(implProvider, commandContext, command);
                }
            }
        }
    }
}