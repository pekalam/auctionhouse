using Common.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.Commands.ChargeUser
{
    public class LockCreditsForBuyNowAuctionCommand : ICommand
    {
        public Guid UserId { get; set; }
        public Guid PaymentId { get; set; }
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
    }
}
