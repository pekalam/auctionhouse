using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Common;
using Core.Common.Command;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public abstract class CommandHandlerBase<T> : IRequestHandler<T, RequestStatus> where T : CommandBase
    {
        private readonly ILogger _logger;

        protected CommandHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        private Task<RequestStatus> TryHandleCommand(T request, CancellationToken cancellationToken)
        {
            try
            {
                return HandleCommand(request, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogDebug(e,"CommandHandler error {@request}", request);
                throw;
            }
        }

        public Task<RequestStatus> Handle(T request, CancellationToken cancellationToken)
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

        protected abstract Task<RequestStatus> HandleCommand(T request, CancellationToken cancellationToken);
    }
}