using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.DomainEvents
{
    public class AuctionLocked : Event
    {
        public Guid AuctionId { get; set; }
        public Guid LockIssuer { get; set; }

        public AuctionLocked() : base("auctionLocked")
        {
        }
    }

    public class AuctionUnlocked : Event
    {
        public Guid AuctionId { get; set; }

        public AuctionUnlocked() : base("auctionUnlocked")
        {
        }
    }
}
