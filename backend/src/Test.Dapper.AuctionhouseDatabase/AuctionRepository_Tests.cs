using Auctions.Domain;
using Auctions.Domain.Repositories;
using Core.Common.Domain.Users;
using Dapper.AuctionhouseDatabase;
using FluentAssertions;
using System;
using Test.Auctions.Domain;
using Users.Domain;
using Xunit;

namespace IntegrationTests
{
    public class AuctionRepository_Tests
    {
        private IAuctionRepository auctionRepository;
        private User user;

        public AuctionRepository_Tests()
        {
            var serverOpt = new MsSqlConnectionSettings()
            {
                //ConnectionString = TestContextUtils.GetParameterOrDefault("sqlserver",
           //"Data Source=.;Initial Catalog=AuctionhouseDatabase;Integrated Security=False;User ID=sa;PWD=Qwerty1234;")
                ConnectionString= "Server=DESKTOP-69UIJIF\\SQLEXPRESS;Database=AuctionhouseDatabase;TrustServerCertificate=True;User ID=sa;Password=qwerty;"
            };
            auctionRepository = new MsSqlAuctionRepository(serverOpt);
            user = User.Create(Username.Create("Test username").Result);
            user.AddCredits(1000);
            user.MarkPendingEventsAsHandled();

        }

        [Fact]
        public void AddAuction_adds_auction_and_FindAuction_reads_it()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();
            auctionRepository.AddAuction(auction);

            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);
            read.Should().BeEquivalentTo(auction);
        }

        [Fact]
        public void AddAuction_adds_auction_UpdateAuction_updates_and_FindAuction_finds_it_by_version()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();

            auction.UpdateDescription("New description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId, auction.Version);

            read.Should().BeEquivalentTo(auction);
        }


        [Fact]
        public void RemoveAuction_removes_added_auction()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();
            auctionRepository.RemoveAuction(auction.AggregateId);

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeNull();
        }

        [Fact]
        public void FindAuction_when_not_found_returns_null()
        {
            var read = auctionRepository.FindAuction(Guid.NewGuid());

            read.Should()
                .BeNull();
        }

        [Fact]
        public void UpdateAuction_updates_auction_with_1_event_and_find_finds_it()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            auctionRepository.AddAuction(auction);
            auction.MarkPendingEventsAsHandled();
            auction.UpdateDescription("new description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }

        [Fact]
        public void UpdateAuction_updates_auction_with_more_than_1_event_and_find_finds_it()
        {
            var auction = new GivenAuction().ValidOfTypeBuyNowAndBid();

            auction.UpdateDescription("new description");

            auctionRepository.UpdateAuction(auction);
            auction.MarkPendingEventsAsHandled();

            var read = auctionRepository.FindAuction(auction.AggregateId);

            read.Should().BeEquivalentTo(auction);
        }
    }
}