using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_RemoveImage
{
    public class RemoveImageCommandHandler : CommandHandlerBase<RemoveImageCommand>
    {
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly ILogger<RemoveImageCommandHandler> _logger;

        public RemoveImageCommandHandler(IAuctionCreateSessionService auctionCreateSessionService, ILogger<RemoveImageCommandHandler> logger) : base(logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(RemoveImageCommand request, CancellationToken cancellationToken)
        {
            var auctionCreateSession = _auctionCreateSessionService.GetExistingSession();

            auctionCreateSession.AddOrReplaceImage(null, request.ImgNum);

            _auctionCreateSessionService.SaveSession(auctionCreateSession);
            _logger.LogDebug($"Removed image {request.ImgNum} from auctionCreateSession user: {auctionCreateSession.Creator.UserName}");

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}