using Auctions.Domain;

namespace Auctions.Tests.Base.Domain.Services.ServiceContracts
{
    public class AuctionPaymentVerificationGiven
    {
        public Auction auction; public UserId buyer; public string paymentMethod;
    }

    public record AuctionPaymentVerificationScenario(AuctionPaymentVerificationGiven Given, Task<bool> Expected);


    public static class AuctionPaymentVerificationContracts
    {
        public static AuctionPaymentVerificationScenario SuccessfulScenario(Auction auction, UserId userId)
        {
            var given = new AuctionPaymentVerificationGiven { auction = auction, buyer = userId, paymentMethod = "test" };
            var expected = true;
            return new(given, Task.FromResult(expected));
        }
    }
}
