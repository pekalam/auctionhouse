﻿using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Query;
using Core.Query.ReadModel;
using Core.Query.Views;
using MediatR;
using MongoDB.Driver;

namespace Core.Query.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsByTagQueryHandler : QueryHandlerBase<TopAuctionsByTagQuery, TopAuctionsInTag>
    {
        private readonly ReadModelDbContext _dbContext;

        public TopAuctionsByTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<TopAuctionsInTag> HandleQuery(TopAuctionsByTagQuery request, CancellationToken cancellationToken)
        {
            var tagsAuctions = _dbContext.TagsAuctionsCollection.Find(t => t.Tag == request.Tag)
                .Skip(request.Page * TopAuctionsByTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsByTagQuery.MAX_PER_PAGE)
                .FirstOrDefault();

            return Task.FromResult(tagsAuctions);
        }
    }
}