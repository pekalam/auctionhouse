using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;

[assembly: InternalsVisibleTo("Test.UnitTests")]
namespace Core.Command.Mediator
{
    public class CommandMediator
    {
        private const string CommandsAssemblyName = "Core.Command";

        private readonly ICommandHandlerMediator _mediator;
        private readonly IImplProvider _implProvider;
        internal static Dictionary<Type, IEnumerable<Action<IImplProvider, CommandBase>>> PreHandleCommandAttributeStrategies;
        internal static Dictionary<Type, IEnumerable<Action<IImplProvider, CommandBase>>> PostHandleCommandAttributeStrategies;

        static CommandMediator()
        {
            LoadCommandAttributeStrategies(CommandsAssemblyName);
        }

        internal static void LoadCommandAttributeStrategies(string commandsAssemblyName)
        {
            PreHandleCommandAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, CommandBase>>>();
            PostHandleCommandAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, CommandBase>>>();
            var commandAttributes = Assembly.Load(commandsAssemblyName)
                .GetTypes()
                .Where(type => type.BaseType == typeof(CommandBase))
                .Where(type => type.GetCustomAttributes(typeof(ICommandAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (ICommandAttribute[]) type.GetCustomAttributes(typeof(ICommandAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in commandAttributes)
            {
                PreHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PreHandleAttributeStrategy)
                        .AsEnumerable();
                PostHandleCommandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.PostHandleAttributeStrategy)
                        .AsEnumerable();
            }
        }

        internal static void InvokePostCommandAttributeStrategies(IImplProvider implProvider, CommandBase commandBase)
        {
            if (PostHandleCommandAttributeStrategies.ContainsKey(commandBase.GetType()))
            {
                foreach (var strategy in PostHandleCommandAttributeStrategies[commandBase.GetType()])
                {
                    strategy?.Invoke(implProvider, commandBase);
                }
            }
        }

        public CommandMediator(ICommandHandlerMediator mediator, IImplProvider implProvider)
        {
            _mediator = mediator;
            _implProvider = implProvider;
        }

        public virtual async Task<RequestStatus> Send(CommandBase commandBase)
        {
            if (PreHandleCommandAttributeStrategies.ContainsKey(commandBase.GetType()))
            {
                foreach (var strategy in PreHandleCommandAttributeStrategies[commandBase.GetType()])
                {
                    strategy?.Invoke(_implProvider, commandBase);
                }
            }

            var requestStatus = await _mediator.Send(commandBase);

            if (!commandBase.HttpQueued && !commandBase.WSQueued)
            {
                InvokePostCommandAttributeStrategies(_implProvider, commandBase);
            }

            return requestStatus;
        }
    }
}