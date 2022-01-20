using Common.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Application.Commands.CreateUserPayments
{
    public class CreateUserPaymentsCommand : ICommand
    {
        public Guid UserId { get; set; }
    }
}
