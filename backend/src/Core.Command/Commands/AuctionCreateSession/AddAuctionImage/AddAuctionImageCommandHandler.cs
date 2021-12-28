using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Domain.Auction.Services;
using Core.Common.Domain.AuctionCreateSession;
using Core.Common.Domain.Auctions;
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
            var file = File.ReadAllBytes(request.TempPath);
            File.Delete(request.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Extension), file);

            var added = _auctionImageService.AddAuctionImage(img);

            request.AuctionCreateSession.AddOrReplaceImage(added, request.ImgNum);

            var response = RequestStatus.CreateFromCommandContext(request.CommandContext, Status.COMPLETED, new Dictionary<string, object>()
            {
                {"imgSz1", added.Size1Id},
                {"imgSz2", added.Size2Id},
                {"imgSz3", added.Size3Id}
            });
            _logger.LogDebug("Image added: {@img}", added);

            return Task.FromResult(response);
        }
    }
}