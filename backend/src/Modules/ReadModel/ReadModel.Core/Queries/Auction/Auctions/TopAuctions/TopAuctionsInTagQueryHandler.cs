﻿using MongoDB.Driver;
using ReadModel.Contracts.Queries.Auction.Auctions.TopAuctions;
using ReadModel.Contracts.Views;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.TopAuctionsByTag
{
    public class TopAuctionsInTagQueryHandler : QueryHandlerBase<TopAuctionsInTagQuery, TopAuctionsInTag>
    {
        private readonly ReadModelDbContext _dbContext;

        public TopAuctionsInTagQueryHandler(ReadModelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<TopAuctionsInTag> HandleQuery(TopAuctionsInTagQuery request, CancellationToken cancellationToken)
        {
            var topByTag = await _dbContext.TagsAuctionsCollection.Find(t => t.Tag == request.Tag)
                .Skip(request.Page * TopAuctionsInTagQuery.MAX_PER_PAGE)
                .Limit(TopAuctionsInTagQuery.MAX_PER_PAGE)
                .FirstOrDefaultAsync();

            return topByTag;
        }
    }
}