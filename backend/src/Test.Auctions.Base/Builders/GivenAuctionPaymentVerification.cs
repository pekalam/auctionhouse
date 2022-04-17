using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.Tests.Base.ServiceContracts;
using Moq;

namespace Auctions.Tests.Base.Builders
{
    public class GivenAuctionPaymentVerification
    {
        public Mock<IAuctionPaymentVerification> CreateMock(AuctionPaymentVerificationScenario scenario)
        {
            var mock = new Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(scenario.Given.auction, scenario.Given.buyer, scenario.Given.paymentMethod))
                .Returns(scenario.Expected);
            return mock;
        }

        public Mock<IAuctionPaymentVerification> CreateAlwaysValidMock()
        {
            var mock = new Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(It.IsAny<Auction>(), It.IsAny<UserId>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            return mock;
        }

        public IAuctionPaymentVerification Create(AuctionPaymentVerificationScenario scenario) => CreateMock(scenario).Object;

        public IAuctionPaymentVerification CreateValid(Auction auction, UserId userId) => Create(AuctionPaymentVerificationContracts.ValidParams(auction, userId));
        public Mock<IAuctionPaymentVerification> CreateValidMock(Auction auction, UserId userId) => CreateMock(AuctionPaymentVerificationContracts.ValidParams(auction, userId));
    }
}
