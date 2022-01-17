using AuctionBids.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;

namespace AuctionBids.Application.Commands.CreateAuctionBids
{
    using AuctionBids.Domain;

    public class CreateAuctionBidsCommandHandler : CommandHandlerBase<CreateAuctionBidsCommand>
    {
        private readonly IAuctionBidsRepository _allAuctionBids;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public CreateAuctionBidsCommandHandler(CommandHandlerBaseDependencies dependencies, IAuctionBidsRepository allAuctionBids, IUnitOfWorkFactory unitOfWorkFactory) : base(ReadModelNotificationsMode.Disabled, dependencies)
        {
            _allAuctionBids = allAuctionBids;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        protected override async Task<RequestStatus> HandleCommand(AppCommand<CreateAuctionBidsCommand> request, IEventOutbox eventOutbox, CancellationToken cancellationToken)
        {
            var auctionBids = AuctionBids.CreateNew(new(request.Command.AuctionId), new(request.Command.Owner));

            using (var uow = _unitOfWorkFactory.Begin())
            {
                _allAuctionBids.Add(auctionBids);
                await eventOutbox.SaveEvents(auctionBids.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                uow.Commit();
            }
            auctionBids.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }
    }
}
