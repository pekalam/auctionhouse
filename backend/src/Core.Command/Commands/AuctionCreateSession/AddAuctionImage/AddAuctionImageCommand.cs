using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;

namespace Core.Command.Commands.AuctionCreateSession.AddAuctionImage
{
    [AuthorizationRequired]
    [InAuctionCreateSession]
    public class AddAuctionImageCommand : ICommand
    {
        [Required]
        public AuctionImageRepresentation Img { get; }

        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        public Common.Domain.AuctionCreateSession.AuctionCreateSession AuctionCreateSession { get; set; }

        public AddAuctionImageCommand(AuctionImageRepresentation img, int imgNum)
        {
            Img = img;
            ImgNum = imgNum;
        }
    }
}