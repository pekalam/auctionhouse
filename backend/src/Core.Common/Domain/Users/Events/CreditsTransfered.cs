using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsTransfered : Event
    {
        public CreditsTransfered() : base("creditsTransfered")
        {
            
        }
    }
}
