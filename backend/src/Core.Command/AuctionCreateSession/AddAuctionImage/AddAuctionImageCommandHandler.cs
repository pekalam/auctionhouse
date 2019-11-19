using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common;
using Core.Common.Auth;
using Core.Common.Command;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventSignalingService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage
{

    public class AddAuctionImageCommandHandler : CommandHandlerBase<AddAuctionImageCommand>
    {
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly ILogger<AddAuctionImageCommandHandler> _logger;
        private readonly AuctionImageService _auctionImageService;

        public AddAuctionImageCommandHandler(IAuctionCreateSessionService auctionCreateSessionService, 
            IEventSignalingService eventSignalingService,
            ILogger<AddAuctionImageCommandHandler> logger,
            AuctionImageService auctionImageService) : base(logger)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _eventSignalingService = eventSignalingService;
            _logger = logger;
            _auctionImageService = auctionImageService;
        }

        protected override Task<CommandResponse> HandleCommand(AddAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var auctionCreateSession = _auctionCreateSessionService.GetExistingSession();

            var added = _auctionImageService.AddAuctionImage(request.Img);

            auctionCreateSession.AddOrReplaceImage(added, request.ImgNum);

            _auctionCreateSessionService.SaveSession(auctionCreateSession);
            _eventSignalingService.TrySendCompletionToUser("auctionImageAdded", request.CorrelationId, auctionCreateSession.Creator, new Dictionary<string, string>()
            {
                {"imgSz1", added.Size1Id},
                {"imgSz2", added.Size2Id},
                {"imgSz3", added.Size3Id}
            });

            var response = new CommandResponse(request.CorrelationId, Status.COMPLETED);
            return Task.FromResult(response);
        }
    }
}