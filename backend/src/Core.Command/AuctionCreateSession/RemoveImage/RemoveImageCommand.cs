using System.ComponentModel.DataAnnotations;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using MediatR;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_RemoveImage
{
    [AuthorizationRequired]
    public class RemoveImageCommand : ICommand
    {
        public int ImgNum { get; }

        public RemoveImageCommand(int imgNum)
        {
            ImgNum = imgNum;
        }
    }
}