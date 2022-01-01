using Common.Application.Commands;
using MediatR;

namespace Common.Application.Mediator
{
    public class ImmediateCommandMediator : CommandMediator
    {
        private readonly IMediator _mediator;
        private readonly IUserIdentityService _userIdentityService;

        public ImmediateCommandMediator(IImplProvider implProvider, IMediator mediator, IUserIdentityService userIdentityService) : base(implProvider)
        {
            _mediator = mediator;
            _userIdentityService = userIdentityService;
        }

        protected override async Task<(RequestStatus, bool)> SendAppCommand<T>(T command)
        {
            var appCommand = new AppCommand<T> { Command = command, CommandContext = CommandContext.CreateNew(_userIdentityService.GetSignedInUserIdentity(), nameof(T)) };
            var status = await _mediator.Send(appCommand);
            return (status, true);
        }
    }
}