using Auctions.Application.CommandAttributes;
using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.AddAuctionImage
{
    [AuthorizationRequired]
    [SaveTempAuctionImage]
    public class AddAuctionImageCommand : ICommand
    {
        [AuctionImage]
        public IFileStreamAccessor Img { get; set; }

        [Required]
        [MaxLength(5)]
        [ValidAuctionImageExtension]
        public string Extension { get; set; }

        [SaveTempPath]
        public string TempPath { get; set; }

        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; set; }
    }
}