using Common.Application.Commands;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Common.Application.Mediator
{
    public abstract class CommandMediator
    {

        private readonly IImplProvider _implProvider;
        internal static Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>> PreHandleCommandAttributeStrategies = null!;
        internal static Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>> PostHandleCommandAttributeStrategies = null!;

        internal static void LoadCommandAttributeStrategies(params string[] commandsAssemblyNames)
        {
            PreHandleCommandAttributeStrategies = new Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>>();
            PostHandleCommandAttributeStrategies = new Dictionary<Type, List<Action<IImplProvider, CommandContext, ICommand>>>();
            var commandAttributes = commandsAssemblyNames.SelectMany(n => Assembly.Load(n).GetTypes()) 
                .Where(type => type.BaseType == typeof(ICommand))
                .Where(type => type.GetCustomAttributes(typeof(ICommandAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (ICommandAttribute[])type.GetCustomAttributes(typeof(ICommandAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in commandAttributes)
            {
                PreHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PreHandleAttributeStrategy)
                        .ToList();
                PostHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PostHandleAttributeStrategy)
                        .ToList();
            }
        }

        internal static void InvokePostCommandAttributeStrategies(IImplProvider implProvider, CommandContext commandContext, ICommand command)
        {
            if (PostHandleCommandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in PostHandleCommandAttributeStrategies[command.GetType()])
                {
                    strategy?.Invoke(implProvider, commandContext, command);
                }
            }
        }

        protected CommandMediator(IImplProvider implProvider)
        {
            _implProvider = implProvider;
        }

        public virtual async Task<RequestStatus> Send<T>(T command) where T : ICommand
        {
            var appCommand = new AppCommand<T>
            {
                Command = command,
                CommandContext = CommandContext.CreateNew(typeof(T).Name),
            };

            if (PreHandleCommandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in PreHandleCommandAttributeStrategies[command.GetType()])
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