using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Common.Domain.Users.Events
{
    public class CreditsWithdrawn : Event
    {
        public decimal CreditsCount { get; }
        public UserIdentity UserIdentity { get; }

        public CreditsWithdrawn(decimal creditsCount, UserIdentity userIdentity) : base("creditsWithdrawn")
        {
            CreditsCount = creditsCount;
            UserIdentity = userIdentity;
        }
    }
}
