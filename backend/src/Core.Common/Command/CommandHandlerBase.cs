using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Exceptions.Command;
using Core.Common.RequestStatusService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Common.Command
{
    //Instead of MediatR pipeline
    public abstract class DecoratedCommandHandlerBase<T> where T : ICommand
    {
        private readonly ILogger _logger;

        protected DecoratedCommandHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        public Task<RequestStatus> Handle(T request, CancellationToken cancellationToken)
        {
            return HandleCommand(request, cancellationToken);
        }

        protected abstract Task<RequestStatus> HandleCommand(T request, CancellationToken cancellationToken);
    }

    public abstract class CommandHandlerBase<T> : IRequestHandler<T, RequestStatus> where T : ICommand
    {
        private readonly ILogger _logger;

        protected CommandHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        public Task<RequestStatus> Handle(T request, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new Collection<ValidationResult>();
            if (Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return HandleCommand(request, cancellationToken);
            }
            else
            {
                _logger.LogDebug("Invalid command");
                foreach (var result in validationResults)
                {
                    _logger.LogDebug(
                        $"validation error membernames: {result.MemberNames.Aggregate((s, s1) => s + s1)} message: {result.ErrorMessage}");
                }

                throw new InvalidCommandException($"Invalid command {request.ToString()}", request.CommandContext);
            }
        }

        protected abstract Task<RequestStatus> HandleCommand(T request, CancellationToken cancellationToken);
    }
}