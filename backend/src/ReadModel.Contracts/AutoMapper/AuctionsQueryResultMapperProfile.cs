using AutoMapper;
using ReadModel.Contracts.Model;
using ReadModel.Contracts.Queries.Auction.Auctions;

namespace ReadModel.Contracts.AutoMapper
{
    public class AuctionsQueryResultMapperProfile : Profile
    {
        public AuctionsQueryResultMapperProfile()
        {
            CreateMap<AuctionRead, AuctionListItem>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
                .ForMember(d => d.Condition, opt => opt.MapFrom(s => s.Product.Condition));

        }
    }
}
