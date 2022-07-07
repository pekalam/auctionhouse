using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsAdded : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserIdentity { get; }

        public CreditsAdded(decimal creditsCount, Guid userIdentity) : base("creditsAdded")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
