using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Command;
using Microsoft.Extensions.Logging;

namespace Core.Command.Handler
{
    public abstract class DecoratedCommandHandlerBase<T> where T : ICommand
    {
        private readonly ILogger _logger;

        protected DecoratedCommandHandlerBase(ILogger logger)
        {
            _logger = logger;
        }

        public Task<RequestStatus> Handle(AppCommand<T> request, CancellationToken cancellationToken)
        {
            return HandleCommand(request, cancellationToken);
        }

        protected abstract Task<RequestStatus> HandleCommand(AppCommand<T> request, CancellationToken cancellationToken);
    }
}