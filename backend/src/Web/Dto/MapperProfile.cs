using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Query.Queries.Auction.Auctions;
using Web.Dto.Queries;

namespace Web.Dto
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<AuctionsQueryDto, AuctionsQuery>();
        }
    
    }
}
