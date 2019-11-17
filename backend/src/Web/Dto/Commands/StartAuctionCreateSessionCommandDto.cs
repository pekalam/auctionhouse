using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Dto.Commands
{
    public class StartAuctionCreateSessionCommandDto
    {
        [Required]
        public string CorrelationId { get; set; }
    }
}
