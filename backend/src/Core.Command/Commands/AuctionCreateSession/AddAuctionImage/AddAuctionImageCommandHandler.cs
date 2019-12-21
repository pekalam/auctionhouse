using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.DomainServices;
using Core.Common.RequestStatusService;
using Microsoft.Extensions.Logging;

namespace Core.Command.Commands.AuctionCreateSession.AddAuctionImage
{

    public class AddAuctionImageCommandHandler : CommandHandlerBase<AddAuctionImageCommand>
    {
        private readonly ILogger<AddAuctionImageCommandHandler> _logger;
        private readonly AuctionImageService _auctionImageService;

        public AddAuctionImageCommandHandler(
            ILogger<AddAuctionImageCommandHandler> logger,
            AuctionImageService auctionImageService) : base(logger)
        {
            _logger = logger;
            _auctionImageService = auctionImageService;
        }

        protected override Task<RequestStatus> HandleCommand(AddAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var added = _auctionImageService.AddAuctionImage(request.Img);

            request.AuctionCreateSession.AddOrReplaceImage(added, request.ImgNum);

//            _requestStatusService.TrySendRequestCompletionToUser("auctionImageAdded", request.CorrelationId, auctionCreateSession.Creator, new Dictionary<string, object>()
//            {
//                {"imgSz1", added.Size1Id},
//                {"imgSz2", added.Size2Id},
//                {"imgSz3", added.Size3Id}
//            });

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED, new Dictionary<string, object>()
            {
                {"imgSz1", added.Size1Id},
                {"imgSz2", added.Size2Id},
                {"imgSz3", added.Size3Id}
            });
            return Task.FromResult(response);
        }
    }
}