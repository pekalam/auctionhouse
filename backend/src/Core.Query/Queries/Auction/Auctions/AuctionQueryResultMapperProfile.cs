using AutoMapper;
using Core.Query.ReadModel;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionQueryResultMapperProfile : Profile
    {
        public AuctionQueryResultMapperProfile()
        {
            CreateMap<AuctionReadModel, AuctionsQueryResult>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));
        }
    }
}
