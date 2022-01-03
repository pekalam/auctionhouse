using Auctions.Domain.Services;
using System.Threading.Tasks;

namespace Test.Auctions.Domain
{
    public class GivenAuctionPaymentVerification
    {
        public IAuctionPaymentVerification Create(AuctionPaymentVerificationScenario scenario)
        {
            var mock = new Moq.Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(scenario.Given.auction, scenario.Given.buyer, scenario.Given.paymentMethod)).Returns(Task.FromResult(scenario.Expected));
            return mock.Object;
        }
    }
}
