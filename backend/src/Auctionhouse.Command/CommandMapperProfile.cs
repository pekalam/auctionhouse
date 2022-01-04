using Auctionhouse.Command.Dto;
using Auctions.Application.Commands.CreateAuction;
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
            CreateMap<CreateAuctionCommandDto, CreateAuctionCommand>();


            CreateMap<SignInCommandDto, SignInCommand>();
            CreateMap<SignUpCommandDto, SignUpCommand>();
            CreateMap<ChangePasswordCommandDto, ChangePasswordCommand>();
            CreateMap<ResetPasswordCommandDto, ResetPasswordCommand>();
            CreateMap<RequestResetPasswordCommandDto, RequestResetPasswordCommand>();
            CreateMap<CheckResetCodeCommandDto, CheckResetCodeCommand>();
        }
    }
}
