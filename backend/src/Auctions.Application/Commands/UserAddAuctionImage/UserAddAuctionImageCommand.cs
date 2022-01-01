using Auctions.Application.CommandAttributes;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.UserAddAuctionImage
{
    [AuthorizationRequired]
    [SaveTempAuctionImage]
    public class UserAddAuctionImageCommand : ICommand
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

        [SignedInUser]
        public Guid SignedInUser { get; set; }

        public UserAddAuctionImageCommand(Guid auctionId, IFileStreamAccessor img, string extension)
        {
            //TODO validator
            if (auctionId.Equals(Guid.Empty)) { throw new InvalidCommandException($"Invalid field AuctionId = {auctionId}"); }
            AuctionId = auctionId;
            Img = img;
            Extension = extension;
        }
    }
}
