using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Command.Handler;
using Core.Common;
using Core.Common.ApplicationServices;
using Core.Common.Command;
using Core.Common.Domain;
using Core.Common.Domain.Users;
using Core.Common.Domain.Users.Events;
using Core.Common.EventBus;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandRollbackHandler : ICommandRollbackHandler
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;

        public BuyCreditsCommandRollbackHandler(ILogger logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public void Rollback(IAppEvent<Event> commandEvent)
        {
            var ev = (CreditsAdded)commandEvent.Event;

            _logger.LogWarning("Canceling buyCredits command for user {@user}", ev.UserIdentity);

            var user = _userRepository.FindUser(ev.UserIdentity);
            user.CancelCredits(ev.CreditsCount);
            _userRepository.UpdateUser(user);
        }
    }

    public class BuyCreditsCommandHandler : DecoratedCommandHandlerBase<BuyCreditsCommand>
    {
        private IUserRepository _userRepository;
        private EventBusService _eventBusService;

        public BuyCreditsCommandHandler(ILogger<BuyCreditsCommandHandler> logger, IUserRepository userRepository,
            EventBusService eventBusService) : base(logger)
        {
            _userRepository = userRepository;
            _eventBusService = eventBusService;
            RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(BuyCreditsCommand), provider =>
            {
                return new BuyCreditsCommandRollbackHandler(provider.Get<ILogger<BuyCreditsCommandRollbackHandler>>(), provider.Get<IUserRepository>());
            });
        }

        protected override Task<RequestStatus> HandleCommand(BuyCreditsCommand request,
            CancellationToken cancellationToken)
        {
            //DEMO
            if (request.Ammount != 15 && request.Ammount != 40 && request.Ammount != 100)
            {
                throw new InvalidCommandException($"Invalid amount value: {request.Ammount}");
            }

            var user = _userRepository.FindUser(request.SignedInUser);

            if (user == null)
            {
                throw new CommandException("Cannot find user");
            }

            user.AddCredits(request.Ammount);
            _userRepository.UpdateUser(user);
            _eventBusService.Publish(user.PendingEvents, request.CommandContext.CorrelationId, request);
            user.MarkPendingEventsAsHandled();

            var status = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.PENDING);
            return Task.FromResult(status);
        }
    }
}