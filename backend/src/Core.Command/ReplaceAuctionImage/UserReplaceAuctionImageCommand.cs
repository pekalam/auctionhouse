using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Exceptions;
using Core.Common.ApplicationServices;
using Core.Common.Domain.Auctions;
using Core.Common.DomainServices;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Command.ReplaceAuctionImage
{

    namespace Core.Command.ReplaceAuctionImage
    {
        public class UserReplaceAuctionImageCommand : IRequest, ICommand
        {
            public Guid AuctionId { get; }
            public AuctionImageRepresentation Img { get; }
            public int ImgNum { get; }
            public CorrelationId CorrelationId { get; }

            public UserReplaceAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, int imgNum, CorrelationId correlationId)
            {
                AuctionId = auctionId;
                Img = img;
                ImgNum = imgNum;
                CorrelationId = correlationId;
            }
        }

        public class UserReplaceAuctionImageCommandHandler : IRequestHandler<UserReplaceAuctionImageCommand>
        {
            private readonly AuctionImageService _auctionImageService;
            private readonly IAuctionRepository _auctionRepository;
            private readonly EventBusService _eventBusService;
            private readonly ILogger<UserReplaceAuctionImageCommandHandler> _logger;

            public UserReplaceAuctionImageCommandHandler(AuctionImageService auctionImageService, IAuctionRepository auctionRepository, EventBusService eventBusService, ILogger<UserReplaceAuctionImageCommandHandler> logger)
            {
                _auctionImageService = auctionImageService;
                _auctionRepository = auctionRepository;
                _eventBusService = eventBusService;
                _logger = logger;
            }

            public Task<Unit> Handle(UserReplaceAuctionImageCommand request, CancellationToken cancellationToken)
            {
                var auction = _auctionRepository.FindAuction(request.AuctionId);
                if (auction == null)
                {
                    _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                    throw new CommandException($"Cannot find auction {request.AuctionId}");
                }

                var newImg = _auctionImageService.AddAuctionImage(request.Img);

                auction.ReplaceImage(newImg, request.ImgNum);

                try
                {
                    _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error while trying to publish events {e.Message}");
                    _auctionImageService.RemoveAuctionImage(newImg);
                    throw;
                }
                _auctionRepository.UpdateAuction(auction);
                return Task.FromResult(Unit.Value);
            }
        }
    }

}
