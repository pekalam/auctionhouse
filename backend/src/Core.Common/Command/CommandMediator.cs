using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core.Common.Attributes;
using MediatR;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("Core.Command")]
namespace Core.Common.Command
{
    public class CommandMediator
    {
        private const string CommandsAssemblyName = "Core.Command";

        private IMediator _mediator;
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
                .Where(type => type.GetInterfaces().Contains(typeof(ICommand)))
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

        public CommandMediator(IMediator mediator, IImplProvider implProvider)
        {
            _mediator = mediator;
            _implProvider = implProvider;
        }

        public async Task<CommandResponse> Send(ICommand command)
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