using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;
using Core.Query.Queries.Auction.Auctions;
using Core.Query.Queries.Auction.Auctions.ByCategory;
using Core.Query.Queries.Auction.Auctions.ByTag;
using Core.Query.Queries.Auction.CommonTags;
using Core.Query.Queries.Auction.SingleAuction;
using Core.Query.Queries.Auction.TopAuctionsByTag;
using Core.Query.Queries.User.UserAuctions;
using Core.Query.Queries.User.UserWonAuctions;
using Web.Dto.Queries;

namespace Web.Dto
{
    public class QueryMapperProfile : Profile
    {
        public QueryMapperProfile()
        {
            CreateMap<AuctionsByCategoryQueryDto, AuctionsByCategoryQuery>();
            CreateMap<AuctionsByTagQueryDto, AuctionsByTagQuery>();
            CreateMap<AuctionQueryDto, AuctionQuery>();
            CreateMap<TopAuctionsByProductNameDto, TopAuctionsByProductNameQuery>();
            CreateMap<CommonTagsQueryDto, CommonTagsQuery>();
            CreateMap<UserWonAuctionsQueryDto, UserWonAuctionsQuery>();
            CreateMap<UserAuctionsQueryDto, UserAuctionsQuery>();
            CreateMap<UserBoughtAuctionsQueryDto, UserBoughtAuctionsQuery>();
            CreateMap<UserWonAuctionsQueryDto, UserWonAuctionsQuery>();
        }

    }
}
