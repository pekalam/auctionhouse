using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain;
using System;
using System.Collections.Generic;

namespace FunctionalTests.Tests.Auctions.Helpers
{
    public class TestCreateAuctionCommandBuilder
    {
        private BuyNowPrice buyNowPrice = new BuyNowPrice(1m);
        private Product product = new Product("testxxx", "descss", Condition.New);
        private AuctionDate startDate = new AuctionDate(DateTime.UtcNow);
        private AuctionDate endDate = new AuctionDate(DateTime.UtcNow.AddDays(1));
        private List<string> CategoryNames = new List<string>() { "1", "2", "3" };
        private Tag[] tags = new[] { new Tag("tag1"), new Tag("tag2"), new Tag("tag3") };
        private AuctionName name = new AuctionName("namewq123");
        private bool buyNowOnly = false;

        public static TestCreateAuctionCommandBuilder GivenCreateAuctionCommand() => new();

        public TestCreateAuctionCommandBuilder WithBuyNowOnly(bool buyNowOnly)
        {
            this.buyNowOnly = buyNowOnly;
            return this;
        }

        public CreateAuctionCommand Build()
        {
            return new CreateAuctionCommand
            {
                BuyNowPrice = buyNowPrice,
                Product = product,
                StartDate = startDate,
                EndDate = endDate,
                Category = CategoryNames,
                Tags = tags,
                Name = name,
                BuyNowOnly = buyNowOnly,
            };
        }
    }
}
