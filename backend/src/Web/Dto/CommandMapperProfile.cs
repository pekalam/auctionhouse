using System;
using AutoMapper;
using Core.Command.CreateAuction;
using Core.Command.UpdateAuction;
using Core.Common.Domain.Products;
using Web.Dto.Commands;

namespace Web.Dto
{
    public class CommandMapperProfile : Profile
    {
        public CommandMapperProfile()
        {
            CreateMap<ProductDto, Product>();
            CreateMap<CreateAuctionCommandDto, CreateAuctionCommand>();
            CreateMap<UpdateAuctionCommandDto, UpdateAuctionCommand>()
                .ForMember(cmd => cmd.AuctionId, opt => opt.MapFrom(dto => Guid.Parse(dto.AuctionId)));
            
        }
    }
}