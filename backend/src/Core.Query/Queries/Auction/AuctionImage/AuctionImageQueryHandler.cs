﻿using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Domain.Auctions;
using Core.Common.Query;
using MediatR;

namespace Core.Query.Queries.Auction.AuctionImage
{
    public class AuctionImageQueryHandler : QueryHandlerBase<AuctionImageQuery, AuctionImageQueryResult>
    {
        private readonly IAuctionImageRepository _auctionImageRepository;

        public AuctionImageQueryHandler(IAuctionImageRepository auctionImageRepository)
        {
            _auctionImageRepository = auctionImageRepository;
        }

        protected override Task<AuctionImageQueryResult> HandleQuery(AuctionImageQuery request, CancellationToken cancellationToken)
        {
            var img = _auctionImageRepository.Find(request.ImageId);
            return Task.FromResult(new AuctionImageQueryResult()
            {
                Img = img
            });
        }
    }
}