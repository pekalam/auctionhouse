using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Core.Common;
using Core.Common.Attributes;
using Core.Common.Command;
using Core.Common.Domain.Products;
using Core.Common.EventBus;
using MediatR;

namespace Core.Command.UpdateAuction
{
    [AuthorizationRequired]
    public class UpdateAuctionCommand : ICommand
    {
        public Guid AuctionId { get; }

        //optional
        public decimal? BuyNowPrice { get; }
        public DateTime? EndDate { get; }
        public List<string> Category { get; }
        public string Description { get; }
        public string[] Tags { get; }
        public string Name { get; }


        public CorrelationId CorrelationId { get; }
    }
}