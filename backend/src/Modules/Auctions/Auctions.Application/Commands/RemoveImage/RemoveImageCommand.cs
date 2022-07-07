using Auctions.Domain;
using Common.Application.Commands;
using Common.Application.Commands.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.Commands.RemoveImage
{
    [AuthorizationRequired]
    public class RemoveImageCommand : ICommand
    {
        [Range(0, AuctionConstantsFactory.DEFAULT_MAX_IMAGES - 1)]
        public int ImgNum { get; }

        public RemoveImageCommand(int imgNum)
        {
            ImgNum = imgNum;
        }
    }
}