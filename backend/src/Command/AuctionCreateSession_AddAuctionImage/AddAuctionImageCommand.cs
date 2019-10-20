using Core.Common.Domain.Auctions;
using Core.Common.EventBus;
using Core.Common.Interfaces;
using MediatR;

namespace Core.Command.AuctionCreateSession_AddAuctionImage
{
    public class AddAuctionImageCommand : IRequest, ICommand
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