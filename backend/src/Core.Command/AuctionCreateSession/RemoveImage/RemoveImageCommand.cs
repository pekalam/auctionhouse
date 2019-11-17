using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_RemoveImage
{
    public class RemoveImageCommand : IRequest, ICommand
    {
        public int ImgNum { get; set; }
    }

    public class RemoveImageCommandHandler : IRequestHandler<RemoveImageCommand>
    {
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<RemoveImageCommandHandler> _logger;

        public RemoveImageCommandHandler(IAuctionCreateSessionService auctionCreateSessionService,
            IUserIdentityService userIdentityService, ILogger<RemoveImageCommandHandler> logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _userIdentityService = userIdentityService;
            _logger = logger;
        }

        public Task<Unit> Handle(RemoveImageCommand request, CancellationToken cancellationToken)
        {
            var userIdentity = _userIdentityService.GetSignedInUserIdentity();
            var auctionCreateSession = _auctionCreateSessionService.GetSessionForSignedInUser();

            auctionCreateSession.AddOrReplaceImage(null, request.ImgNum);

            _auctionCreateSessionService.SaveSessionForSignedInUser(auctionCreateSession);
            _logger.LogDebug($"Removed image {request.ImgNum} from auctionCreateSession user: {userIdentity.UserName}");
            return Task.FromResult(Unit.Value);
        }
    }
}