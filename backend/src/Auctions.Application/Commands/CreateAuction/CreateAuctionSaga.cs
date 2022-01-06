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
        private readonly Lazy<IAuctionImageRepository> _auctionImages;
        private readonly Lazy<IAuctionEndScheduler> _auctionEndScheduler;
        private readonly Lazy<IAuctionRepository> _auctions;

        public CreateAuctionSaga(Lazy<IAuctionImageRepository> auctionImages, Lazy<IAuctionEndScheduler> auctionEndScheduler, Lazy<IAuctionRepository> auctions)
        {
            _auctionImages = auctionImages;
            _auctionEndScheduler = auctionEndScheduler;
            _auctions = auctions;
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
            var createAuctionService = new CreateAuctionService(_auctionImages, _auctionEndScheduler, _auctions, Data.CreateAuctionServiceData);
            createAuctionService.EndCreate(new Domain.AuctionBidsId(message.AuctionBidsId));
            createAuctionService.Commit();
            return CompleteAsync();
        }
    }
}
