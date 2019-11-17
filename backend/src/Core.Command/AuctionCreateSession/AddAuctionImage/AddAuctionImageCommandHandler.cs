using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Core.Common.Auth;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
using Core.Common.EventSignalingService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage
{

    public class AddAuctionImageCommandHandler : IRequestHandler<AddAuctionImageCommand>
    {
        private readonly IAuctionImageRepository _imageRepository;
        private readonly IAuctionImageSizeConverterService _imageConverterService;
        private readonly IAuctionCreateSessionService _auctionCreateSessionService;
        private readonly IEventSignalingService _eventSignalingService;
        private readonly IUserIdentityService _userIdentityService;
        private readonly ILogger<AddAuctionImageCommandHandler> _logger;

        public AddAuctionImageCommandHandler(IAuctionImageRepository imageRepository, IAuctionImageSizeConverterService imageConverterService, IAuctionCreateSessionService auctionCreateSessionService, IEventSignalingService eventSignalingService, IUserIdentityService userIdentityService, ILogger<AddAuctionImageCommandHandler> logger)
        {
            _imageRepository = imageRepository;
            _imageConverterService = imageConverterService;
            _auctionCreateSessionService = auctionCreateSessionService;
            _eventSignalingService = eventSignalingService;
            _userIdentityService = userIdentityService;
            _logger = logger;
        }

        private string GenerateImageId(AuctionImageSize size) => $"auction-img-{Guid.NewGuid().ToString()}-{size}";

        private void AddImgOfSize(AuctionImageSize size, string imageId, AddAuctionImageCommand cmd)
        {
            AuctionImageRepresentation converted = null;
            try
            {
                converted = _imageConverterService.ConvertTo(cmd.Img, size);
            }
            catch (Exception)
            {
                _logger.LogError($"Cannot convert image correlationId: {cmd.CorrelationId}");
                throw;
            }

            try
            {
                _imageRepository.Add(imageId, converted);
            }
            catch (Exception)
            {
                _logger.LogError($"Cannot add image correlationId: {cmd.CorrelationId}");
                throw;
            }
        }

        public Task<Unit> Handle(AddAuctionImageCommand request, CancellationToken cancellationToken)
        {
            var userIdentity = _userIdentityService.GetSignedInUserIdentity();
            var auctionCreateSession = _auctionCreateSessionService.GetSessionForSignedInUser();

            var auctionImg = new AuctionImage(
                GenerateImageId(AuctionImageSize.SIZE1),
                GenerateImageId(AuctionImageSize.SIZE2),
                GenerateImageId(AuctionImageSize.SIZE3));

            AddImgOfSize(AuctionImageSize.SIZE1, auctionImg.Size1Id, request);
            AddImgOfSize(AuctionImageSize.SIZE2, auctionImg.Size2Id, request);
            AddImgOfSize(AuctionImageSize.SIZE3, auctionImg.Size3Id, request);

            auctionCreateSession.AddOrReplaceImage(auctionImg, request.ImgNum);

            _auctionCreateSessionService.SaveSessionForSignedInUser(auctionCreateSession);
            _eventSignalingService.TrySendCompletionToUser("auctionImageAdded", request.CorrelationId, userIdentity, new Dictionary<string, string>()
            {
                {"imgSz1", auctionImg.Size1Id},
                {"imgSz2", auctionImg.Size2Id},
                {"imgSz3", auctionImg.Size3Id}
            });
            return Task.FromResult(Unit.Value);
        }
    }
}