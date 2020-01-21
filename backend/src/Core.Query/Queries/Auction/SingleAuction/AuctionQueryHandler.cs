using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query.Exceptions;
using Core.Query.Mediator;
using Core.Query.ReadModel;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.SingleAuction
{
    public class AuctionQueryHandler : QueryHandlerBase<AuctionQuery, AuctionRead>
    {
        private readonly ReadModelDbContext _readModelDbContext;

        public AuctionQueryHandler(ReadModelDbContext readModelDbContext)
        {
            _readModelDbContext = readModelDbContext;
        }

        protected override async Task<AuctionRead> HandleQuery(AuctionQuery request, CancellationToken cancellationToken)
        {
            var filter = Builders<AuctionRead>.Filter.Eq(model => model.AuctionId, request.AuctionId);
            var upd = Builders<AuctionRead>.Update.Inc(f => f.Views, 1);

            //TODO FindOneAndUpdate
            AuctionRead auction = await _readModelDbContext.AuctionsReadModel
                .Find(filter)
                .FirstOrDefaultAsync();
            if (auction == null)
            {
                throw new ResourceNotFoundException($"Cannot find auction with id: {request.AuctionId}");
            }

            await _readModelDbContext.AuctionsReadModel.UpdateManyAsync(filter, upd);

            return auction;
        }
    }
}
