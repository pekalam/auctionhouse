using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Web.Dto.Commands
{
    public class RemoveImageCommandDto
    {
        [FromQuery(Name= "num")]
        public int ImgNum { get; set; }
    }
}
