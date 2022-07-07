using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.DomainEvents
{
    public class UserCreated : Event
    {
        public Guid UserId { get; }

        public string Username { get; }

        public decimal InitialCredits { get; }

        public UserCreated(Guid userId, string username, decimal initialCredits) : base("userCreated")
        {
            UserId = userId;
            Username = username;
            InitialCredits = initialCredits;
        }
    }
}
