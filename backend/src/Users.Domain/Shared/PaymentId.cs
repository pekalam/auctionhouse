using Core.DomainFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.Shared
{
    public class PaymentId : GuidId<PaymentId>
    {
        public PaymentId(Guid value) : base(value)
        {
        }
    }
}
