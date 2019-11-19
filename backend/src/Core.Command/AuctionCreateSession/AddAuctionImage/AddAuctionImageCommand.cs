using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage
{
    [AuthorizationRequired]
    public class AddAuctionImageCommand : ICommand
    {
        public AuctionImageRepresentation Img { get; }
        public CorrelationId CorrelationId { get; }
        public int ImgNum { get; }

        public AddAuctionImageCommand(AuctionImageRepresentation img, CorrelationId correlationId, int imgNum)
        {
            Img = img;
            CorrelationId = correlationId;
            ImgNum = imgNum;
        }
    }
}