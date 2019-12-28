using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsAdded : Event
    {
        public decimal CreditsCount { get; }
        public UserIdentity UserIdentity { get; }

        public CreditsAdded(decimal creditsCount, UserIdentity userIdentity) : base("creditsAdded")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
