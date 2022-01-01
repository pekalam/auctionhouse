using Common.Application;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace Common.Application.Commands
{
    public abstract class CommandHandlerBase<T> : IRequestHandler<AppCommand<T>, RequestStatus> where T : ICommand
    {
        private readonly ILogger _logger;

        protected CommandHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        private async Task<RequestStatus> TryHandleCommand(AppCommand<T> request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleCommand(request, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "CommandHandler error {@request}", request);
                throw;
            }
        }

        public virtual Task<RequestStatus> Handle(AppCommand<T> request, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new Collection<ValidationResult>();

            _logger.LogTrace("Handling command {name}", typeof(T).Name);
            if (Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return TryHandleCommand(request, cancellationToken);
            }
            else
            {
                _logger.LogDebug("Invalid command {validationResults}", validationResults);
                throw new InvalidCommandException($"Invalid command {request}", request.CommandContext);
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, CancellationToken cancellationToken);
    }
}