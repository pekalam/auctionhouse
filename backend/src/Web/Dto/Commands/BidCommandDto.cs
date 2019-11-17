using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class BidCommandDto
    {
        [Required]
        public string AuctionId { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string CorrelationId { get; set; }
    }
}
