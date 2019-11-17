using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class AuctionQueryDto
    {
        [FromQuery(Name = "auctionId")]
        [Required]
        public string AuctionId { get; set; }
    }
}
