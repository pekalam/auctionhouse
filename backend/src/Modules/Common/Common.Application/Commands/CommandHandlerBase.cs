using Common.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Common.Application.Commands
{
    //TODO: remove
    public class CommandHandlerBaseDependencies
    {
        /// <summary>
        /// Test ctor
        /// </summary>
        internal CommandHandlerBaseDependencies()
        {

        }

        public CommandHandlerBaseDependencies(ILogger<RequestStatus> logger, IEventOutbox eventOutbox, EventOutboxSender eventOutboxSender, IEventOutboxSavedItems eventOutboxSavedItems, ICommandHandlerCallbacks commandHandlerLi)
        {
            Logger = logger;
            EventOutbox = eventOutbox;
            EventOutboxSender = eventOutboxSender;
            EventOutboxSavedItems = eventOutboxSavedItems;
            CommandHandlerCallbacks = commandHandlerLi;
        }

        public ILogger<RequestStatus> Logger { get; set; }
        public IEventOutbox EventOutbox { get; set; }
        public EventOutboxSender EventOutboxSender { get; set; }
        public IEventOutboxSavedItems EventOutboxSavedItems { get; set; }
        public ICommandHandlerCallbacks CommandHandlerCallbacks { get; set; }
    }


    public abstract class CommandHandlerBase<T> : IRequestHandler<AppCommand<T>, RequestStatus> where T : ICommand
    {
        private readonly ILogger<RequestStatus> _logger;
        // subclass provides value of this field to decide which mode should be used when saving command status
        private readonly IEventOutbox _eventOutbox;
        private readonly EventOutboxSender _eventOutboxSender;
        private readonly IEventOutboxSavedItems _eventOutboxSavedItems;
        private readonly ICommandHandlerCallbacks _commandHandlerCallbacks;


        protected CommandHandlerBase(CommandHandlerBaseDependencies dependencies)
        {
            _logger = dependencies.Logger;
            _eventOutbox = dependencies.EventOutbox;
            _eventOutboxSender = dependencies.EventOutboxSender;
            _eventOutboxSavedItems = dependencies.EventOutboxSavedItems;
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

                await _eventOutboxSender.SendEvents(_eventOutboxSavedItems.SavedOutboxStoreItems);
                await _commandHandlerCallbacks.OnEventsSent(_eventOutboxSavedItems.SavedOutboxStoreItems);

                if (requestStatus.Status == Status.COMPLETED) Activity.Current.TraceOkStatus();
                if (requestStatus.Status == Status.FAILED) Activity.Current.TraceErrorStatus();
                if (requestStatus.Status == Status.PENDING) Activity.Current.TraceUnsetStatus();
                return requestStatus;
            }
            else
            {
                Activity.Current.TraceErrorStatus("Invalid command validation results");
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandException($"Invalid command {request}", request.CommandContext);
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, IEventOutbox eventOutbox, CancellationToken cancellationToken);
    }
}