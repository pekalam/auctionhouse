using Auctions.Domain;
using Auctions.DomainEvents;
using AutoMapper;
using ReadModel.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadModel.Core.EventConsumers.Auction
{
    internal class AuctionCreatedEventMapperProfile : Profile
    {
        public AuctionCreatedEventMapperProfile()
        {
            //CreateMap<UserId, UserIdentityRead>() //TODO
            //    .ForMember(read => read.UserId, opt => opt.MapFrom(identity => identity.ToString()));
            //CreateMap<Bid, BidRead>() //TODO
            //    .ForMember(read => read.AuctionId, opt => opt.MapFrom(bid => bid.AuctionId.ToString()));
            CreateMap<AuctionCreated, AuctionRead>(MemberList.Source)
                .ForMember(r => r.Version, opt => opt.MapFrom(e => e.AggVersion))
                .ForSourceMember(e => e.EventName, o => o.DoNotValidate())
                .ForSourceMember(e => e.ProductName, o => o.DoNotValidate())
                .ForSourceMember(e => e.ProductCondition, o => o.DoNotValidate())
                .ForSourceMember(e => e.ProductDescription, o => o.DoNotValidate())
                .ForSourceMember(e => e.AuctionImagesSize1Id, o => o.DoNotValidate())
                .ForSourceMember(e => e.AuctionImagesSize2Id, o => o.DoNotValidate())
                .ForSourceMember(e => e.AuctionImagesSize3Id, o => o.DoNotValidate())
                .ForMember(r => r.Category, opt => opt.Ignore())
                .ForMember(r => r.Owner, opt => opt.MapFrom(e => new UserIdentityRead
                {
                    UserId = e.Owner.ToString(),
                }))
                .ForMember(r => r.Product, opt => opt.MapFrom(e => new ProductRead
                {
                    Name = e.ProductName,
                    Description = e.ProductDescription,
                    Condition = e.ProductCondition,
                    CanonicalName = Product.CanonicalizeProductName(e.ProductName),
                }))
                .ForMember(r => r.AuctionImages, opt => opt.MapFrom(e => e.AuctionImagesSize1Id
                .Select((id, i) => id != null ? new AuctionImageRead
                {
                    ImgNum = i,
                    Size1Id = e.AuctionImagesSize1Id[i]!,
                    Size2Id = e.AuctionImagesSize2Id[i]!,
                    Size3Id = e.AuctionImagesSize3Id[i]!,
                } : null)
                .Where(x => x != null)
                .ToArray()));
            //.ForMember(read => read.AuctionId, opt => opt.MapFrom(created => created.AuctionId.ToString()))
            //.ForMember(read => read.Id, opt => opt.AllowNull())
            //.ForMember(read => read.Version, opt => opt.MapFrom(created => created.AggVersion))
            //.ForMember(read => read.DateCreated, opt => opt.Ignore())
            //.ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
