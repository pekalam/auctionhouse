using AutoMapper;
using ReadModel.Core.Model;

namespace ReadModel.Core.Queries.Auction.Auctions
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
