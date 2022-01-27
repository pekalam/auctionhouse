using Auctions.Domain;
using Auctions.Domain.Services;
using System.Threading.Tasks;
using Test.Auctions.Base.ServiceContracts;

namespace Test.Auctions.Base.Builders
{
    public class GivenAuctionPaymentVerification
    {
        public IAuctionPaymentVerification Create(AuctionPaymentVerificationScenario scenario)
        {
            var mock = new Moq.Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(scenario.Given.auction, scenario.Given.buyer, scenario.Given.paymentMethod)).Returns(Task.FromResult(scenario.Expected));
            return mock.Object;
        }

        public IAuctionPaymentVerification CreateValid(Auction auction, UserId userId) => Create(AuctionPaymentVerificationContracts.ValidParams(auction, userId)); 
    }
}
