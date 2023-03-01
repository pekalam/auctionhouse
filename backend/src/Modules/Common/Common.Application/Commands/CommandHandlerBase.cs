using Common.Application.Commands.Callbacks;
using Common.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Common.Application.Commands
{
    public class CommandHandlerBaseDependencies
    {
        /// <summary>
        /// Test ctor
        /// </summary>
        internal CommandHandlerBaseDependencies()
        {

        }

        public CommandHandlerBaseDependencies(ILogger<RequestStatus> logger, IEventOutbox eventOutbox, EventOutboxItemsSender eventOutboxItemsSender, 
            ICommandHandlerCallbacks commandHandlerCallbacks)
        {
            Logger = logger;
            EventOutbox = eventOutbox;
            EventOutboxItemsSender = eventOutboxItemsSender;
            CommandHandlerCallbacks = commandHandlerCallbacks;
        }

        public ILogger<RequestStatus> Logger { get; set; }
        public IEventOutbox EventOutbox { get; set; }
        public EventOutboxItemsSender EventOutboxItemsSender { get; set; }
        public ICommandHandlerCallbacks CommandHandlerCallbacks { get; set; }
    }


    public abstract class CommandHandlerBase<T> : IRequestHandler<AppCommand<T>, RequestStatus> where T : ICommand
    {
        private readonly ILogger<RequestStatus> _logger;
        private readonly IEventOutbox _eventOutbox;
        private readonly EventOutboxItemsSender _eventOutboxItemsSender;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;


        protected CommandHandlerBase(CommandHandlerBaseDependencies dependencies)
        {
            _logger = dependencies.Logger;
            _eventOutbox = dependencies.EventOutbox;
            _eventOutboxItemsSender = dependencies.EventOutboxItemsSender;
            _commandHandlerCallbacks = dependencies.CommandHandlerCallbacks;
        }

        private async Task<RequestStatus> HandleCommandInternal(AppCommand<T> request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleCommand(request, _eventOutbox, cancellationToken);
            }
            catch (Exception e)
            {
                Activity.Current.TraceErrorStatus("Command handling error");
                _logger.LogDebug(e, "CommandHandler error {@request}", request);
                throw;
            }
        }

        public virtual async Task<RequestStatus> Handle(AppCommand<T> request, CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope("{CorrelationId}", request.CommandContext.CorrelationId.Value);
            using var activity = Tracing.StartActivity(typeof(T).Name, request.CommandContext.CorrelationId);

            var validationContext = new ValidationContext(request.Command);
            var validationResults = new Collection<ValidationResult>();

            _logger.LogDebug("Handling command {name}", typeof(T).Name);
            if (Validator.TryValidateObject(request.Command, validationContext, validationResults, true))
            {
                await _commandHandlerCallbacks.OnExecute(request);

                //when exception is thrown then it should be handled by api
                //failed status can result in events being sent
                var requestStatus = await HandleCommandInternal(request, cancellationToken);
                await _commandHandlerCallbacks.OnCompleted(request);

                await _eventOutboxItemsSender.Send(_eventOutbox.SavedOutboxItems);
                await _commandHandlerCallbacks.OnEventsSent(_eventOutbox.SavedOutboxItems);

                if (requestStatus.Status == Status.COMPLETED) Activity.Current.TraceOkStatus();
                if (requestStatus.Status == Status.FAILED) Activity.Current.TraceErrorStatus();
                if (requestStatus.Status == Status.PENDING) Activity.Current.TraceUnsetStatus();
                return requestStatus;
            }
            else
            {
                Activity.Current.TraceErrorStatus("Invalid command validation results");
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandDataException($"Invalid command data {request}", request.CommandContext);
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, IEventOutbox eventOutbox, CancellationToken cancellationToken);
    }
}