using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Common;
using Core.Common.Domain.Auctions;

namespace Core.Command.Commands.AuctionCreateSession.AddAuctionImage
{
    [AuthorizationRequired]
    [InAuctionCreateSession]
    [SaveTempAuctionImage]
    public class AddAuctionImageCommand : ICommand
    {
        [AuctionImage]
        public IFileStreamAccessor Img { get; set; }

        [Required]
        [MaxLength(5)]
        [ValidAuctionImageExtension]
        public string Extension { get; }

        [SaveTempPath]
        public string TempPath { get; set; }

        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        public Common.Domain.AuctionCreateSession.AuctionCreateSession AuctionCreateSession { get; set; }

        public AddAuctionImageCommand(IFileStreamAccessor img, int imgNum, string extension)
        {
            Img = img;
            ImgNum = imgNum;
            Extension = extension;
        }
    }
}