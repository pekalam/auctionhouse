using Auctions.Application.Commands.CreateAuction;
using Auctions.Domain;
using System;
using System.Collections.Generic;

namespace FunctionalTests.Builders
{
    public class CreateAuctionCommandBuilder
    {
        private readonly BuyNowPrice buyNowPrice = new BuyNowPrice(1m);
        private readonly Product product = new Product("testxxx", "descss", Condition.New);
        private readonly AuctionDate startDate = new AuctionDate(DateTime.UtcNow);
        private readonly AuctionDate endDate = new AuctionDate(DateTime.UtcNow.AddDays(1));
        private readonly List<string> CategoryNames = new List<string>() { "1", "2", "3" };
        private readonly Tag[] tags = new[] { new Tag("tag1"), new Tag("tag2"), new Tag("tag3") };
        private readonly AuctionName name = new AuctionName("namewq123");
        private readonly bool buyNowOnly = false;

        public static CreateAuctionCommandBuilder GivenCreateAuctionCommand() => new();

        public CreateAuctionCommand Build()
        {
            return new CreateAuctionCommand(
                buyNowPrice, product, startDate, endDate, CategoryNames, tags, name, buyNowOnly
                );
        }
    }
}
