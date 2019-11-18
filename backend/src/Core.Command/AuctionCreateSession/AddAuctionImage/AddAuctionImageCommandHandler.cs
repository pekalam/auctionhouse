using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventSignalingService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage
{

    public class AddAuctionImageCommandHandler : IRequestHandler<AddAuctionImageCommand>
    {
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<AddAuctionImageCommandHandler> _logger;
        private readonly AuctionImageService _auctionImageService;

        public AddAuctionImageCommandHandler(IAuctionCreateSessionService auctionCreateSessionService, IEventSignalingService eventSignalingService, IUserIdentityService userIdentityService, ILogger<AddAuctionImageCommandHandler> logger, AuctionImageService auctionImageService)
        {
            _auctionCreateSessionService = auctionCreateSessionService;
            _eventSignalingService = eventSignalingService;
            _userIdentityService = userIdentityService;
            _logger = logger;
            _auctionImageService = auctionImageService;
        }


        public Task<Unit> Handle(AddAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var userIdentity = _userIdentityService.GetSignedInUserIdentity();
            var auctionCreateSession = _auctionCreateSessionService.GetSessionForSignedInUser();

            var added = _auctionImageService.AddAuctionImage(request.Img);

            auctionCreateSession.AddOrReplaceImage(added, request.ImgNum);

            _auctionCreateSessionService.SaveSessionForSignedInUser(auctionCreateSession);
            _eventSignalingService.TrySendCompletionToUser("auctionImageAdded", request.CorrelationId, userIdentity, new Dictionary<string, string>()
            {
                {"imgSz1", added.Size1Id},
                {"imgSz2", added.Size2Id},
                {"imgSz3", added.Size3Id}
            });
            return Task.FromResult(Unit.Value);
        }
    }
}