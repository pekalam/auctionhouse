using Auctions.Domain;
using Auctions.Domain.Services;
using Common.Application;
using Common.Application.Commands;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.AddAuctionImage
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

            var requestStatus = RequestStatus.CreatePending(request.CommandContext);
            requestStatus.SetExtraData(new Dictionary<string, object>()
            {
                {"imgSz1", added.Size1Id},
                {"imgSz2", added.Size2Id},
                {"imgSz3", added.Size3Id}
            });
            requestStatus.MarkAsCompleted();
            _logger.LogDebug("Image added: {@img}", added);

            return Task.FromResult(requestStatus);
        }
    }
}