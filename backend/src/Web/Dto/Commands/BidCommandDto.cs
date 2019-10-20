using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class BidCommandDto
    {
        [FromBody]
        public string AuctionId { get; set; }
        [FromBody]
        public decimal Price { get; set; }
        [FromBody]
        public string CorrelationId { get; set; }
    }
}
