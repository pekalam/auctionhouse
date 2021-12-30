using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.AuctionCreateSession.RemoveImage
{
    public class RemoveImageCommandHandler : CommandHandlerBase<RemoveImageCommand>
    {
        private readonly IAuctionCreateSessionStore _auctionCreateSessionService;
        private readonly ILogger<RemoveImageCommandHandler> _logger;

        public RemoveImageCommandHandler(IAuctionCreateSessionStore auctionCreateSessionService, ILogger<RemoveImageCommandHandler> logger) : base(logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _logger = logger;
        }

        protected override Task<RequestStatus> HandleCommand(AppCommand<RemoveImageCommand> request, CancellationToken cancellationToken)
        {
            var auctionCreateSession = _auctionCreateSessionService.GetExistingSession();

            auctionCreateSession.AddOrReplaceImage(null, request.Command.ImgNum);

            _auctionCreateSessionService.SaveSession(auctionCreateSession);
            _logger.LogDebug("Removed image {num} from auctionCreateSession user: {@user}", request.Command.ImgNum, auctionCreateSession.Creator);

            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.MarkAsCompleted();
            return Task.FromResult(requestStatus);
        }
    }
}