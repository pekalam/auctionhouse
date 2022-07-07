using Common.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Application.Commands.SetPaymentStatusToFailed
{
    public class SetPaymentStatusToFailedCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
