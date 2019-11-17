using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Command.CreateAuction;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.Queries.Auction.Auctions.ByTag;
using Web.Dto.Commands;
using Web.Dto.Queries;

namespace Web.Dto
{
    public class QueryMapperProfile : Profile
    {
        public QueryMapperProfile()
        {
            CreateMap<AuctionsByCategoryQueryDto, AuctionsByCategoryQuery>();
            CreateMap<AuctionsByTagQueryDto, AuctionsByTagQuery>();
            CreateMap<ProductDto, Product>();
            CreateMap<CreateAuctionCommandDto, CreateAuctionCommand>();
        }
    
    }
}
