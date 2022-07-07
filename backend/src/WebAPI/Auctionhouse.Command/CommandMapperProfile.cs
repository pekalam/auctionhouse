using Auctionhouse.Command.Dto;
using Auctions.Application.Commands.AddAuctionImage;
using Auctions.Application.Commands.BuyNow;
using Auctions.Application.Commands.CreateAuction;
using Auctions.Application.Commands.RemoveImage;
using Auctions.Application.Commands.UpdateAuction;
using Auctions.Domain;
using AutoMapper;
using Users.Application.Commands.ChangePassword;
using Users.Application.Commands.CheckResetCode;
using Users.Application.Commands.RequestResetPassword;
using Users.Application.Commands.ResetPassword;
using Users.Application.Commands.SignIn;
using Users.Application.Commands.SignUp;

namespace Auctionhouse.Command
{
    public class CommandMapperProfile : Profile
    {
        public CommandMapperProfile()
        {
            CreateMap<ProductDto, Product>(MemberList.Source)
                .ConstructUsing((dto) => new Product(dto.Name, dto.Description, (Condition)dto.Condition));
            CreateMap<CreateAuctionCommandDto, CreateAuctionCommand>(MemberList.Source);


            CreateMap<SignInCommandDto, SignInCommand>();
            CreateMap<SignUpCommandDto, SignUpCommand>();
            CreateMap<ChangePasswordCommandDto, ChangePasswordCommand>(MemberList.Source);
            CreateMap<ResetPasswordCommandDto, ResetPasswordCommand>(MemberList.Source);
            CreateMap<RequestResetPasswordCommandDto, RequestResetPasswordCommand>();
            CreateMap<CheckResetCodeCommandDto, CheckResetCodeCommand>();
            CreateMap<BuyNowCommandDto, BuyNowCommand>();

            CreateMap<AddAuctionImageCommandDto, AddAuctionImageCommand>(MemberList.Source)
                .ForMember(dto => dto.Img, cfg => cfg.Ignore());
            CreateMap<RemoveImageCommandDto, RemoveImageCommand>(MemberList.Source);
            CreateMap<UpdateAuctionCommandDto, UpdateAuctionCommand>(MemberList.Source);
        }
    }
}
