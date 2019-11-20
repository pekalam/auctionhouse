using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Queries
{
    public class CommonTagsQueryDto
    {
        [FromQuery(Name = "tag")]
        public string Tag { get; set; }
    }
}
