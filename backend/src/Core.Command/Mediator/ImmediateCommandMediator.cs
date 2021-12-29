using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using MediatR;
using System.Threading.Tasks;

namespace Core.Command.Mediator
{
    public class ImmediateCommandMediator : CommandMediator
    {
        private readonly IMediator _mediator;
        private readonly IUserIdentityService _userIdentityService;

        public ImmediateCommandMediator(IImplProvider implProvider, IMediator mediator) : base(implProvider)
        {
            _mediator = mediator;
        }

        protected override async Task<(RequestStatus, bool)> SendAppCommand<T>(T command)
        {
            var appCommand = new AppCommand<T> { Command = command, CommandContext = CommandContext.CreateNew(_userIdentityService.GetSignedInUserIdentity(), nameof(T))};
            var status = await _mediator.Send(appCommand);
            return (status, true);
        }
    }
}