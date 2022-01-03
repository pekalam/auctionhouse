﻿using Auctions.Domain;

namespace Test.Auctions.Domain
{
    public class AuctionPaymentVerificationContractArgs
    {
        public Auction auction; public UserId buyer; public string paymentMethod;
    }

    public record AuctionPaymentVerificationScenario(AuctionPaymentVerificationContractArgs Given, bool Expected);


    public static class AuctionPaymentVerificationContracts
    {
        public static AuctionPaymentVerificationScenario ValidParams(Auction auction, UserId userId)
        {
            var given = new AuctionPaymentVerificationContractArgs { auction = auction, buyer = userId, paymentMethod = "test" };
            var expected = true;
            return new(given, expected);
        }
    }
}