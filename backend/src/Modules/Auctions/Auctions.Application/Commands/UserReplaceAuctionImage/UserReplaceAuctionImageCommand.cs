using Auctions.Application.CommandAttributes;
using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.UserReplaceAuctionImage
{
    [AuthorizationRequired]
    [SaveTempAuctionImage]
    public class UserReplaceAuctionImageCommand : ICommand
    {
        public Guid AuctionId { get; }

        [AuctionImage]
        public IFileStreamAccessor Img { get; set; }

        [SaveTempPath]
        public string TempPath { get; set; }

        [Required]
        [MaxLength(5)]
        [ValidAuctionImageExtension]
        public string Extension { get; }

        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public UserReplaceAuctionImageCommand(Guid auctionId, IFileStreamAccessor img, int imgNum, string extension)
        {
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Img = img;
            ImgNum = imgNum;
            Extension = extension;
        }
    }
}