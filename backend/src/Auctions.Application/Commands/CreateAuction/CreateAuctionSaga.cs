using Auctions.Domain.Repositories;
using Auctions.Domain.Services;
using Auctions.DomainEvents;
using Chronicle;
using AuctionBidsEvents = AuctionBids.DomainEvents.Events;

namespace Auctions.Application.Commands.CreateAuction
{
    public class CreateAuctionSagaData
    {
        public CreateAuctionServiceData CreateAuctionServiceData { get; set; } = null!;
    }

    public class CreateAuctionSaga : Saga<CreateAuctionSagaData>, ISagaStartAction<AuctionCreated>, ISagaAction<AuctionBidsEvents.V1.AuctionBidsCreated>
    {
        public const string ServiceDataKey = "ServiceData";
        private readonly Func<IAuctionRepository> _auctionRepositoryFactory;

        public CreateAuctionSaga(Func<IAuctionRepository> auctionRepositoryFactory)
        {
            _auctionRepositoryFactory = auctionRepositoryFactory;
        }

        public Task CompensateAsync(AuctionCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task CompensateAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(AuctionCreated message, ISagaContext context)
        {
            if (!context.TryGetMetadata(ServiceDataKey, out var metadata))
            {
                throw new InvalidOperationException();
            }
            Data.CreateAuctionServiceData = (CreateAuctionServiceData)metadata.Value;
            return Task.CompletedTask;
        }

        public Task HandleAsync(AuctionBidsEvents.V1.AuctionBidsCreated message, ISagaContext context)
        {
            var createAuctionService = new CreateAuctionService(_auctionRepositoryFactory(), Data.CreateAuctionServiceData);
            createAuctionService.EndCreate();
            return CompleteAsync();
        }
    }
}
