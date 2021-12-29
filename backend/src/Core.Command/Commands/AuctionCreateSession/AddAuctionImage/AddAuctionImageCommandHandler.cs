using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Command.Handler;
using Core.Command.Mediator;
using Core.Common;
using Core.Common.Command;
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

        protected override Task<RequestStatus> HandleCommand(AppCommand<AddAuctionImageCommand> request, CancellationToken cancellationToken)
        {
            var file = File.ReadAllBytes(request.Command.TempPath);
            File.Delete(request.Command.TempPath);

            var img = new AuctionImageRepresentation(new AuctionImageMetadata(request.Command.Extension), file);

            var added = _auctionImageService.AddAuctionImage(img);

            request.Command.AuctionCreateSession.AddOrReplaceImage(added, request.Command.ImgNum);

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