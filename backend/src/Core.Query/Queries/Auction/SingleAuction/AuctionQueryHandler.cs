using System.Threading;
using System.Threading.Tasks;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.SingleAuction
{
    public class AuctionQueryHandler : IRequestHandler<AuctionQuery, AuctionReadModel>
    {
        private readonly ReadModelDbContext _readModelDbContext;

        public AuctionQueryHandler(ReadModelDbContext readModelDbContext)
        {
            _readModelDbContext = readModelDbContext;
        }

        public async Task<AuctionReadModel> Handle(AuctionQuery request, CancellationToken cancellationToken)
        {
            var filter = Builders<AuctionReadModel>.Filter.Eq(model => model.AuctionId, request.AuctionId);
            var upd = Builders<AuctionReadModel>.Update.Inc(f => f.Views, 1);
            
            //TODO FindOneAndUpdate
            AuctionReadModel auction = await _readModelDbContext.AuctionsReadModel
                .Find(filter)
                .FirstAsync();

            await _readModelDbContext.AuctionsReadModel.UpdateManyAsync(filter, upd);
            
            return auction;
        }
    }
}
