using System;
using AutoMapper;
using Core.Command.Bid;
using Core.Command.Commands;
using Core.Command.Commands.BuyNow;
using Core.Command.Commands.CancelBid;
using Core.Command.Commands.ChangePassword;
using Core.Command.Commands.CheckResetCode;
using Core.Command.Commands.RequestResetPassword;
using Core.Command.Commands.ResetPassword;
using Core.Command.Commands.SignIn;
using Core.Command.Commands.UpdateAuction;
using Core.Command.CreateAuction;
using Core.Command.SignUp;
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
            CreateMap<BuyNowCommandDto, BuyNowCommand>()
                .ForMember(cmd => cmd.AuctionId, opt => opt.MapFrom(dto => Guid.Parse(dto.AuctionId)));
            CreateMap<UpdateAuctionCommandDto, UpdateAuctionCommand>()
                .ForMember(cmd => cmd.AuctionId, opt => opt.MapFrom(dto => Guid.Parse(dto.AuctionId)));
            CreateMap<CancelBidCommandDto, CancelBidCommand>()
                .ForMember(cmd => cmd.AuctionId, opt => opt.MapFrom(dto => Guid.Parse(dto.AuctionId)))
                .ForMember(cmd => cmd.BidId, opt => opt.MapFrom(dto => Guid.Parse(dto.BidId)));
            CreateMap<BidCommandDto, BidCommand>()
                .ForMember(cmd => cmd.AuctionId, opt => opt.MapFrom(dto => Guid.Parse(dto.AuctionId)));


            CreateMap<SignInCommandDto, SignInCommand>();
            CreateMap<SignUpCommandDto, SignUpCommand>();
            CreateMap<ChangePasswordCommandDto, ChangePasswordCommand>();
            CreateMap<ResetPasswordCommandDto, ResetPasswordCommand>();
            CreateMap<RequestResetPasswordCommandDto, RequestResetPasswordCommand>();
            CreateMap<CheckResetCodeCommandDto, CheckResetCodeCommand>();
        }
    }
}