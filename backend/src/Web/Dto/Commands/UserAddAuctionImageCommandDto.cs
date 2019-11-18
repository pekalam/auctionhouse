using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class UserAddAuctionImageCommandDto
    {
        [FromForm(Name = "auction-id")]
        public string AuctionId { get; set; }
        [FromForm(Name = "img")]
        [Required]
        public IFormFile Img { get; set; }
        [FromForm(Name = "correlation-id")]
        [Required]
        public string CorrelationId { get; set; }
    }
}
