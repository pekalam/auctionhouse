using Core.Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.DomainEvents
{
    public class AuctionBidsAdded : AuctionEvent
    {
        public Guid AuctionId { get; set; }
        public Guid AuctionBidsId { get; set; }

        public AuctionBidsAdded() : base("auctionBidsAdded")
        {
        }
    }
}
