using Auctions.Domain;
using Auctions.Domain.Services;
using Auctions.Tests.Base.Domain.Services.ServiceContracts;
using Moq;

namespace Auctions.Tests.Base.Domain.Services.TestDoubleBuilders
{
    public class GivenAuctionPaymentVerification
    {
        public Mock<IAuctionPaymentVerification> BuildMock(AuctionPaymentVerificationScenario scenario)
        {
            var mock = new Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(scenario.Given.auction, scenario.Given.buyer, scenario.Given.paymentMethod))
                .Returns(scenario.Expected);
            return mock;
        }

        public Mock<IAuctionPaymentVerification> BuildAlwaysValidMock()
        {
            var mock = new Mock<IAuctionPaymentVerification>();
            mock.Setup(f => f.Verification(It.IsAny<Auction>(), It.IsAny<UserId>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            return mock;
        }

        public IAuctionPaymentVerification Build(AuctionPaymentVerificationScenario scenario) => BuildMock(scenario).Object;

        public IAuctionPaymentVerification BuildValid(Auction auction, UserId userId) => Build(AuctionPaymentVerificationContracts.SuccessfulScenario(auction, userId));
        public Mock<IAuctionPaymentVerification> BuildValidMock(Auction auction, UserId userId) => BuildMock(AuctionPaymentVerificationContracts.SuccessfulScenario(auction, userId));
    }
}
