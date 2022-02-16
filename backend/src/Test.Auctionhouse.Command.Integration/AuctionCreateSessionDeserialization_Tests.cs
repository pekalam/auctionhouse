using Auctionhouse.Command.Adapters;
using Auctions.Domain;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Auctionhouse.Command.Integration
{
    public class AuctionCreateSessionDeserialization_Tests
    {
        [Fact]
        public void f()
        {
            var session = new AuctionCreateSession(
                    new AuctionImages(), DateTime.UtcNow, UserId.New());

            var bytes = AuctionCreateSessionStoreSerialization.SerializeSession(session);
            Assert.True(bytes.Length > 0);

            var deserialized = AuctionCreateSessionStoreSerialization.DeserializeSession(bytes);
            deserialized.Should().BeEquivalentTo(session);
        }


    }
}
