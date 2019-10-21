using System.Threading;
using System.Threading.Tasks;
using Core.Common.Domain.Auctions;
using MediatR;

namespace Core.Query.Queries.Auction.AuctionImage
{
    public class AuctionImageQuery : IRequest<AuctionImageQueryResult>
    {
        public string ImageId { get; set; }
    }

    public class AuctionImageQueryResult
    {
        public AuctionImageRepresentation Img { get; set; }
    }

    public class AuctionImageQueryHandler : IRequestHandler<AuctionImageQuery, AuctionImageQueryResult>
    {
        private readonly IAuctionImageRepository _auctionImageRepository;

        public AuctionImageQueryHandler(IAuctionImageRepository auctionImageRepository)
        {
            _auctionImageRepository = auctionImageRepository;
        }

        public Task<AuctionImageQueryResult> Handle(AuctionImageQuery request, CancellationToken cancellationToken)
        {
            var img = _auctionImageRepository.FindImage(request.ImageId);
            return Task.FromResult(new AuctionImageQueryResult()
            {
                Img = img
            });
        }
    }
}
