using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.ApplicationServices;
using Core.Common.Attributes;
using Core.Common.Auth;
using Core.Common.EventBus;
using MediatR;

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("Core.Command")]
namespace Core.Common.Command
{
    public class QueuedCommandMediator : CommandMediator
    {
        public QueuedCommandMediator(EventBusCmdHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }
    }

    public class EventBusCmdHandlerMediator : ICommandHandlerMediator
    {
        private EventBusService _eventBusService;
        private IUserIdentityService _userIdentityService;


        public EventBusCmdHandlerMediator(EventBusService eventBusService, IUserIdentityService userIdentityService)
        {
            _eventBusService = eventBusService;
            _userIdentityService = userIdentityService;
        }


        public Task<RequestStatus> Send(ICommand command)
        {
            var correlationId = new CorrelationId(Guid.NewGuid().ToString());
            var signedInUser = _userIdentityService.GetSignedInUserIdentity();
            command.CommandContext = new CommandContext(correlationId, signedInUser);

            _eventBusService.SendQueuedCommand(command);

            var requestStatus = new RequestStatus(correlationId, Status.PENDING);
            return Task.FromResult(requestStatus);
        }
    }

    public interface ICommandHandlerMediator
    {
        Task<RequestStatus> Send(ICommand command);
    }

    public class MediatRCommandHandlerMediator : ICommandHandlerMediator
    {
        private IMediator _mediator;

        public MediatRCommandHandlerMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<RequestStatus> Send(ICommand command)
        {
            return _mediator.Send(command);
        }
    }

    public class ImmediateCommandMediator : CommandMediator
    {
        public ImmediateCommandMediator(MediatRCommandHandlerMediator mediator, IImplProvider implProvider) : base(mediator, implProvider)
        {
        }
    }

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