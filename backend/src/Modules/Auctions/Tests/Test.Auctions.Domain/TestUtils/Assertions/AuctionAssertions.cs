using Auctions.Domain;
using Core.Common.Domain;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auctions.Domain.Tests.Assertions
{
    public static class AuctionAssertions
    {
        public static void ShouldEmitNoEvents(this Auction auction)
        {
            auction.PendingEvents.Should().BeEmpty();
        }

        public static Event ShouldEmitSingleEvent(this Auction auction)
        {
            auction.PendingEvents.Should().HaveCount(1);
            return auction.PendingEvents.First();
        }
    }
}
