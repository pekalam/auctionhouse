using AuctionBids.Domain.Repositories;
using Common.Application;
using Common.Application.Commands;
using Common.Application.Events;

namespace AuctionBids.Application.Commands.CreateAuctionBids
{
    using AuctionBids.Domain;
    using AuctionBids.Domain.Shared;
    using Core.DomainFramework;

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
            var categoryIds = new AuctionCategoryIds(request.Command.CategoryId, request.Command.SubCategoryId, request.Command.SubSubCategoryId);
            var auctionBids = AuctionBids.CreateNew(new(request.Command.AuctionId), categoryIds, new(request.Command.Owner));
            // try to detect if already called - happy path
            if (_allAuctionBids.WithAuctionId(new(request.Command.AuctionId)) is not null)
            {
                return RequestStatus.CreateCompleted(request.CommandContext);
            }

            using (var uow = _unitOfWorkFactory.Begin())
            {
                if (!TryAddAuctionBids(auctionBids)) //try to detect if already called - pessimistic path
                {
                    return RequestStatus.CreateCompleted(request.CommandContext);
                }
                await eventOutbox.SaveEvents(auctionBids.PendingEvents, request.CommandContext, ReadModelNotificationsMode.Saga);
                uow.Commit();
            }
            auctionBids.MarkPendingEventsAsHandled();

            return RequestStatus.CreateCompleted(request.CommandContext);
        }

        private bool TryAddAuctionBids(AuctionBids auctionBids)
        {
            try
            {
                _allAuctionBids.Add(auctionBids);
                return true;
            }
            catch (ConcurrentInsertException e)
            {
                return false;
            }
        }
    }
}
