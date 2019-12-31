using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Users;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandHandler : DecoratedCommandHandlerBase<BuyCreditsCommand>
    {
        private IUserRepository _userRepository;
        private EventBusService _eventBusService;

        public BuyCreditsCommandHandler(ILogger<BuyCreditsCommandHandler> logger, IUserRepository userRepository,
            EventBusService eventBusService) : base(logger)
        {
            _userRepository = userRepository;
            _eventBusService = eventBusService;
        }

        protected override Task<RequestStatus> HandleCommand(BuyCreditsCommand request,
            CancellationToken cancellationToken)
        {
            var user = _userRepository.FindUser(request.SignedInUser);

            if (user == null)
            {
                throw new CommandException("Cannot find user");
            }

            user.AddCredits(1000.0m);
            _userRepository.UpdateUser(user);

            _eventBusService.Publish(user.PendingEvents, request.CommandContext.CorrelationId, request);
            user.MarkPendingEventsAsHandled();

            return Task.FromResult(RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED));
        }
    }
}