using Common.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserPayments.Application.Commands.CompletePayment
{
    public class CompletePaymentCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
    }
}
