using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Web.Dto.Queries
{
    public class UserWonAuctionsQueryDto
    {
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 0;
    }
}
