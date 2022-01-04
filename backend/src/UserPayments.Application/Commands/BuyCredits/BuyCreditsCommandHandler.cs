using Common.Application;
using Common.Application.Commands;
using Core.Common.Domain;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.BuyCredits
{
    public class BuyCreditsCommandHandler : CommandHandlerBase<BuyCreditsCommand>
    {
        public BuyCreditsCommandHandler(ILogger<BuyCreditsCommandHandler> logger) : base(logger)
        {
        }

        //private IUserRepository _userRepository;
        //private EventBusService _eventBusService;

        //public BuyCreditsCommandHandler(ILogger<BuyCreditsCommandHandler> logger, IUserRepository userRepository,
        //    EventBusService eventBusService) : base(logger)
        //{
        //    _userRepository = userRepository;
        //    _eventBusService = eventBusService;
        //    RollbackHandlerRegistry.RegisterCommandRollbackHandler(nameof(BuyCreditsCommand), provider =>
        //    {
        //        return new BuyCreditsCommandRollbackHandler(provider.Get<ILogger<BuyCreditsCommandRollbackHandler>>(), provider.Get<IUserRepository>());
        //    });
        //}

        protected override Task<RequestStatus> HandleCommand(AppCommand<BuyCreditsCommand> request,
            CancellationToken cancellationToken)
        {
            //DEMO
            //if (request.Command.Ammount != 15 && request.Command.Ammount != 40 && request.Command.Ammount != 100)
            //{
            //    throw new InvalidCommandException($"Invalid amount value: {request.Command.Ammount}");
            //}

            //var user = _userRepository.FindUser(request.Command.SignedInUser);

            //if (user == null)
            //{
            //    throw new CommandException("Cannot find user");
            //}

            //user.AddCredits(request.Command.Ammount);
            //_userRepository.UpdateUser(user);
            //_eventBusService.Publish(user.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Immediate);
            //user.MarkPendingEventsAsHandled();

            return Task.FromResult(RequestStatus.CreatePending(request.CommandContext));
        }
    }
}