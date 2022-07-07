using Adapter.Dapper.AuctionhouseDatabase;
using AuctionBids.Domain.Repositories;
using AuctionBids.Domain.Shared;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Dapper.AuctionhouseDatabase
{
    public class AuctionBidsRepository_Tests
    {
        IAuctionBidsRepository auctionBids;

        public AuctionBidsRepository_Tests()
        {
            var serverOpt = new AuctionhouseRepositorySettings()
            {
                ConnectionString = "Server=127.0.0.1;Database=AuctionhouseDatabase;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234;"
            };
            auctionBids = new MsSqlAuctionBidsRepository(serverOpt);
        }

        [Fact]
        public void Adds_auction_to_repository_and_finds_it_by_auctionId()
        {
            var bids = AuctionBids.Domain.AuctionBids.CreateNew(new AuctionId(Guid.NewGuid()), new AuctionCategoryIds(1,2,3), new UserId(Guid.NewGuid()));

            auctionBids.Add(bids);
            bids.MarkPendingEventsAsHandled();

            auctionBids.WithAuctionId(bids.AuctionId).Should().BeEquivalentTo(bids);
        }
    }
}
