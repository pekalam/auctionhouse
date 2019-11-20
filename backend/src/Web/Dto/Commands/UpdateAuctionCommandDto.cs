using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Common.EventBus;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class UpdateAuctionCommandDto
    {
        public string AuctionId { get; set; }
        public string CorrelationId { get; set; }

        //optional
        public decimal? BuyNowPrice { get; set; }
        public DateTime? EndDate { get; set; }
        //

        public List<string> Category { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string Name { get; set; }
    }

}
