﻿using System.ComponentModel.DataAnnotations;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Auctions;

namespace Core.Command.AuctionCreateSession.AuctionCreateSession_RemoveImage
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