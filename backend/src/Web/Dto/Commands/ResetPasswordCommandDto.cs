using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Dto.Commands
{
    public class ResetPasswordCommandDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetCode { get; set; }
    }
}
