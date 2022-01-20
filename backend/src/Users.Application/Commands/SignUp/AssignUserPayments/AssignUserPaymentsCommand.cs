using Common.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Commands.SignUp.AssignUserPayments
{
    public class AssignUserPaymentsCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid UserPaymentsId { get; set; }
    }
}
