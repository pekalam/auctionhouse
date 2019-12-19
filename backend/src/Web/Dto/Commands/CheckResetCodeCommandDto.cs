using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Dto.Commands
{
    public class CheckResetCodeCommandDto
    {
        public string ResetCode { get; set; }
        public string Email { get; set; }
    }
}
