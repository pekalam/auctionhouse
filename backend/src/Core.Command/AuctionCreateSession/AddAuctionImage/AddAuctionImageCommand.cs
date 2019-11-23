using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;
using Core.Common.EventBus;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_AddAuctionImage
{
    [AuthorizationRequired]
    public class AddAuctionImageCommand : ICommand
    {
        [Required]
        public AuctionImageRepresentation Img { get; }

        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        public AddAuctionImageCommand(AuctionImageRepresentation img, int imgNum)
        {
            Img = img;
            ImgNum = imgNum;
        }
    }
}