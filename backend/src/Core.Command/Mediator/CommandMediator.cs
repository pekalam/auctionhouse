using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;

[assembly: InternalsVisibleTo("UnitTests")]
namespace Core.Command.Mediator
{
    public class CommandMediator
    {
        private const string CommandsAssemblyName = "Core.Command";

        private ICommandHandlerMediator _mediator;
        private IImplProvider _implProvider;
        internal static Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>> _commandAttributeStrategies;

        static CommandMediator()
        {
            LoadCommandAttributeStrategies(CommandsAssemblyName);
        }

        internal static void LoadCommandAttributeStrategies(string commandsAssemblyName)
        {
            _commandAttributeStrategies = new Dictionary<Type, IEnumerable<Action<IImplProvider, ICommand>>>();
            var commandAttributes = Assembly.Load(commandsAssemblyName)
                .GetTypes()
                .Where(type => type.BaseType == typeof(ICommand))
                .Where(type => type.GetCustomAttributes(typeof(ICommandAttribute), false).Length > 0)
                .Select(type => new
                {
                    CommandType = type,
                    CommandAttributes = (ICommandAttribute[]) type.GetCustomAttributes(typeof(ICommandAttribute), false)
                })
                .ToArray();

            foreach (var commandAttribute in commandAttributes)
            {
                _commandAttributeStrategies[commandAttribute.CommandType] =
                    commandAttribute.CommandAttributes.OrderBy(attribute => attribute.Order)
                        .Select(attribute => attribute.AttributeStrategy)
                        .AsEnumerable();
            }
        }

        public CommandMediator(ICommandHandlerMediator mediator, IImplProvider implProvider)
        {
            _mediator = mediator;
            _implProvider = implProvider;
        }

        public virtual async Task<RequestStatus> Send(ICommand command)
        {
            if (_commandAttributeStrategies.ContainsKey(command.GetType()))
            {
                foreach (var strategy in _commandAttributeStrategies[command.GetType()])
                {
                    strategy.Invoke(_implProvider, command);
                }
            }

            return await _mediator.Send(command);
        }
    }
}