using AutoMapper;
using Core.Query.ReadModel;

namespace Core.Query.Queries.Auction.Auctions
{
    public class AuctionsQueryResultMapperProfile : Profile
    {
        public AuctionsQueryResultMapperProfile()
        {
            CreateMap<AuctionReadModel, AuctionsQueryResult>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Condition, opt => opt.MapFrom(s => s.Product.Condition));

        }
    }
}
