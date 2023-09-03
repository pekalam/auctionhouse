using Auctionhouse.Query.Queries;
using AutoMapper;
using ReadModel.Contracts.Queries.Auction.Auctions.ByCategory;
using ReadModel.Contracts.Queries.Auction.Auctions.ByTag;
using ReadModel.Contracts.Queries.Auction.Auctions.TopAuctions;
using ReadModel.Contracts.Queries.Auction.CommonTags;
using ReadModel.Contracts.Queries.Auction.SingleAuction;
using ReadModel.Contracts.Queries.User.UserAuctions;
using ReadModel.Contracts.Queries.User.UserBoughtAuctions;
using ReadModel.Contracts.Queries.User.UserWonAuctions;

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
