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

namespace Core.Command.RemoveAuctionImage
{
    namespace Core.Command.RemoveAuctionImage
    {
        public class UserRemoveAuctionImageCommand : IRequest, ICommand
        {
            public Guid AuctionId { get; }
            public AuctionImageRepresentation Img { get; }
            public int ImgNum { get; }
            public CorrelationId CorrelationId { get; }

            public UserRemoveAuctionImageCommand(Guid auctionId, AuctionImageRepresentation img, int imgNum,
                CorrelationId correlationId)
            {
                AuctionId = auctionId;
                Img = img;
                ImgNum = imgNum;
                CorrelationId = correlationId;
            }
        }

        public class UserRemoveAuctionImageCommandHandler : IRequestHandler<UserRemoveAuctionImageCommand>
        {
            private readonly IAuctionRepository _auctionRepository;
            private readonly EventBusService _eventBusService;
            private readonly ILogger<UserRemoveAuctionImageCommandHandler> _logger;

            public UserRemoveAuctionImageCommandHandler(IAuctionRepository auctionRepository,
                EventBusService eventBusService, ILogger<UserRemoveAuctionImageCommandHandler> logger)
            {
                _auctionRepository = auctionRepository;
                _eventBusService = eventBusService;
                _logger = logger;
            }

            public Task<Unit> Handle(UserRemoveAuctionImageCommand request, CancellationToken cancellationToken)
            {
                var auction = _auctionRepository.FindAuction(request.AuctionId);
                if (auction == null)
                {
                    _logger.LogDebug($"Cannot find auction {request.AuctionId}");
                    throw new CommandException($"Cannot find auction {request.AuctionId}");
                }

                auction.RemoveImage(request.ImgNum);


                _eventBusService.Publish(auction.PendingEvents, request.CorrelationId, request);
                _auctionRepository.UpdateAuction(auction);
                return Task.FromResult(Unit.Value);
            }
        }
    }
}