using Auctions.Application.Commands.BuyNow;
using Auctions.Application.Commands.CreateAuction;
using Users.Application.Commands.SignUp;

namespace Auctionhouse.Command
{
    internal static class SagaTypeSerialization //TODO temporary solution - consider changing serialization logic in chronicle.integrations or change library
    {
        public static Type GetSagaType(string sagaType) => sagaType switch
        {
            nameof(BuyNowSaga) => typeof(BuyNowSaga),
            nameof(CreateAuctionSaga) => typeof(CreateAuctionSaga),
            nameof(SignUpSaga) => typeof(SignUpSaga),
            _ => throw new NotImplementedException(),
        };
    }
}
