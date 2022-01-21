using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Events
{
    public class CreditsLocked : Event
    {
        public Guid LockedFundsId { get; set; }
        public decimal Amount { get; set; }
        public Guid UserId { get; set; }

        public CreditsLocked() : base("creditsUnlocked")
        {
        }
    }
}
