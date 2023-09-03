using AutoMapper;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.MostViewed;

namespace ReadModel.Core.Queries.Auction.MostViewed
{
    public class MostViewedAuctionsResultMapperProfile : Profile
    {
        public MostViewedAuctionsResultMapperProfile()
        {
            CreateMap<AuctionRead, MostViewedAuctionsResult>();
        }
    }
}