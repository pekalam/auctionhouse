using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.DomainEvents
{
    public class AuctionEnded : AuctionEvent
    {
        public Guid AuctionId { get; set; }

        public AuctionEnded() : base("auctionEnded")
        {
        }
    }
}
