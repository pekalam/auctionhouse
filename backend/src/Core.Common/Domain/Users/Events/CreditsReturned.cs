using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsReturned : Event
    {
        public decimal CreditsCount { get; }
        public UserIdentity UserIdentity { get; }

        public CreditsReturned(decimal creditsCount, UserIdentity userIdentity) : base("creditsReturned")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
