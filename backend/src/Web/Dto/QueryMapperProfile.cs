using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.Queries.Auction.Auctions.ByTag;
using Web.Dto.Queries;

namespace Web.Dto
{
    public class QueryMapperProfile : Profile
    {
        public QueryMapperProfile()
        {
            CreateMap<AuctionsByCategoryQueryDto, AuctionsByCategoryQuery>();
            CreateMap<AuctionsByTagQueryDto, AuctionsByTagQuery>();
        }
    
    }
}
