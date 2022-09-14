using Auctionhouse.Query.Queries;
using AutoMapper;
using ReadModel.Core.Queries.Auction.Auctions.ByCategory;
using ReadModel.Core.Queries.Auction.Auctions.ByTag;
using ReadModel.Core.Queries.Auction.CommonTags;
using ReadModel.Core.Queries.Auction.SingleAuction;
using ReadModel.Core.Queries.Auction.TopAuctionsByTag;
using ReadModel.Core.Queries.User.UserAuctions;
using ReadModel.Core.Queries.User.UserBoughtAuctions;
using ReadModel.Core.Queries.User.UserWonAuctions;

namespace Auctionhouse.Query
{
    public class QueryMapperProfile : Profile
    {
        public QueryMapperProfile()
        {
            CreateMap<AuctionsByCategoryQueryDto, AuctionsByCategoryQuery>(MemberList.Source);
            CreateMap<AuctionsByTagQueryDto, AuctionsByTagQuery>(MemberList.Source);
            CreateMap<AuctionQueryDto, AuctionQuery>(MemberList.Source);
            CreateMap<TopAuctionsByProductNameDto, TopAuctionsByProductNameQuery>(MemberList.Source);
            CreateMap<CommonTagsQueryDto, CommonTagsQuery>(MemberList.Source);
            CreateMap<UserAuctionsQueryDto, UserAuctionsQuery>(MemberList.Source);
            CreateMap<UserBoughtAuctionsQueryDto, UserBoughtAuctionsQuery>(MemberList.Source);
            CreateMap<UserWonAuctionsQueryDto, UserWonAuctionsQuery>(MemberList.Source);
        }

    }
}
