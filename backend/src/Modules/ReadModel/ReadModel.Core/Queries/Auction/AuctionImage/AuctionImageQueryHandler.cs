using ReadModel.Core.Services;

namespace ReadModel.Core.Queries.Auction.AuctionImage
{
    public class AuctionImageQueryHandler : QueryHandlerBase<AuctionImageQuery, AuctionImageQueryResult>
    {
        private readonly IAuctionImageReadRepository _auctionImageRepository;

        public AuctionImageQueryHandler(IAuctionImageReadRepository auctionImageRepository)
        {
            _auctionImageRepository = auctionImageRepository;
        }

        protected override Task<AuctionImageQueryResult> HandleQuery(AuctionImageQuery request,
            CancellationToken cancellationToken)
        {
            var img = _auctionImageRepository.Find(request.ImageId);

            if (img == null)
            {
                return Task.FromResult(new AuctionImageQueryResult());
            }
            else
            {
                return Task.FromResult(new AuctionImageQueryResult()
                {
                    Img = img
                });
            }
        }
    }
}