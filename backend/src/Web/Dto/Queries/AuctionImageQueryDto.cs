using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class AuctionImageQueryDto
    {
        [FromQuery(Name = "img")]
        public string ImageId { get; set; }
    }
}
