using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsReturned : Event
    {
        public decimal CreditsCount { get; }
        public Guid UserIdentity { get; }

        public CreditsReturned(decimal creditsCount, Guid userIdentity) : base("creditsReturned")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
